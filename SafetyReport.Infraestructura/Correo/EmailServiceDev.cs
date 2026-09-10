using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Me.SendMail;
using Microsoft.Identity.Client;
using Microsoft.Kiota.Abstractions.Authentication;
using SafetyReport.Application.Puertos.Informe;
using SafetyReport.Infrastructure.Almacenamiento;

namespace SafetyReport.Infrastructure.Correo
{
    // Envio via Graph con auth delegada (MSAL public client + Device Code Flow), usado
    // en Development porque la cuenta de pruebas es una cuenta personal de Outlook/Hotmail,
    // y los permisos de aplicacion (app-only) de Graph no existen para cuentas personales.
    // El token cache de MSAL se persiste en S3 (no en disco local) para sobrevivir redeploys.
    public class EmailServiceDev : IInformeEmailSender
    {
        private static readonly string[] _scopes = ["Mail.Send"];
        private static readonly TimeSpan _cacheTtl = TimeSpan.FromHours(4);

        private readonly IPublicClientApplication _app;
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
        private readonly string _cacheKey;
        private readonly ILogger<EmailServiceDev> _logger;

        private byte[]? _cacheEnMemoria;
        private DateTime _cacheEnMemoriaFecha = DateTime.MinValue;

        public EmailServiceDev(EmailDevConfig config, AwsConfig awsConfig, IAmazonS3 s3Client, ILogger<EmailServiceDev> logger)
        {
            _logger = logger;
            _s3Client = s3Client;
            _bucketName = awsConfig.BucketName;
            _cacheKey = config.TokenCacheS3Key;

            _app = PublicClientApplicationBuilder
                .Create(config.ClientId)
                .WithAuthority($"https://login.microsoftonline.com/{config.Tenant}")
                .Build();

            _app.UserTokenCache.SetBeforeAccessAsync(async args =>
            {
                var bytes = await ObtenerCacheAsync();
                if (bytes is not null)
                    args.TokenCache.DeserializeMsalV3(bytes);
            });
            _app.UserTokenCache.SetAfterAccessAsync(async args =>
            {
                if (args.HasStateChanged)
                {
                    var bytes = args.TokenCache.SerializeMsalV3();
                    _cacheEnMemoria = bytes;
                    _cacheEnMemoriaFecha = DateTime.UtcNow;
                    await SubirCacheAsync(bytes);
                }
            });
        }

        // Evita ir a S3 en cada envio: el token cache cambia solo cuando MSAL rota el
        // refresh token, asi que una copia en memoria de hasta 4 horas es segura y
        // sigue quedando escrita en S3 (fuente de verdad) apenas cambia.
        private async Task<byte[]?> ObtenerCacheAsync()
        {
            if (_cacheEnMemoria is not null && DateTime.UtcNow - _cacheEnMemoriaFecha < _cacheTtl)
                return _cacheEnMemoria;

            var bytes = await DescargarCacheAsync();
            _cacheEnMemoria = bytes;
            _cacheEnMemoriaFecha = DateTime.UtcNow;
            return bytes;
        }

        private async Task<byte[]?> DescargarCacheAsync()
        {
            try
            {
                var response = await _s3Client.GetObjectAsync(_bucketName, _cacheKey);
                using var ms = new MemoryStream();
                await response.ResponseStream.CopyToAsync(ms);
                return ms.ToArray();
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        private async Task SubirCacheAsync(byte[] data)
        {
            await _s3Client.PutObjectAsync(new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = _cacheKey,
                InputStream = new MemoryStream(data)
            });
        }

        public async Task EnviarNotificacionInformeAsync(string correoDestino, NotificacionInformeEmailDetalle detalle)
        {
            var accounts = await _app.GetAccountsAsync();
            var cuenta = accounts.FirstOrDefault();

            if (cuenta == null)
            {
                // Requiere el login inicial (Device Code Flow) para poblar el token cache en S3.
                throw new InvalidOperationException("No hay una sesion de correo autenticada. Es necesario rehacer el login de la cuenta de envio.");
            }

            var resultado = await _app.AcquireTokenSilent(_scopes, cuenta).ExecuteAsync();

            var authProvider = new BaseBearerTokenAuthenticationProvider(new StaticTokenProvider(resultado.AccessToken));
            var graphClient = new GraphServiceClient(authProvider);

            var mensaje = GraphMailMessageBuilder.Construir(correoDestino, detalle);

            await graphClient.Me.SendMail.PostAsync(new SendMailPostRequestBody
            {
                Message = mensaje,
                SaveToSentItems = true
            });
        }
    }
}
