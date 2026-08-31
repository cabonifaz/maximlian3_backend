using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Me.SendMail;
using Microsoft.Graph.Models;
using Microsoft.Identity.Client;
using Microsoft.Kiota.Abstractions.Authentication;
using SafetyReport.Models;

namespace SafetyReport.Handlers
{
    public class EmailService : IEmailService
    {
        private static readonly string[] _scopes = ["Mail.Send"];

        private readonly IPublicClientApplication _app;
        private readonly string _cacheFile;
        private readonly ILogger<EmailService> _logger;

        public EmailService(EmailConfig config, ILogger<EmailService> logger)
        {
            _logger = logger;
            _cacheFile = config.TokenCachePath;

            _app = PublicClientApplicationBuilder
                .Create(config.ClientId)
                .WithAuthority($"https://login.microsoftonline.com/{config.Tenant}")
                .Build();

            _app.UserTokenCache.SetBeforeAccess(args =>
            {
                if (File.Exists(_cacheFile))
                    args.TokenCache.DeserializeMsalV3(File.ReadAllBytes(_cacheFile));
            });
            _app.UserTokenCache.SetAfterAccess(args =>
            {
                if (args.HasStateChanged)
                    File.WriteAllBytes(_cacheFile, args.TokenCache.SerializeMsalV3());
            });
        }

        public async Task EnviarNotificacionInformeAsync(string correoDestino, NotificacionInformeEmailDetalle detalle)
        {
            var accounts = await _app.GetAccountsAsync();
            var cuenta = accounts.FirstOrDefault();

            if (cuenta == null)
            {
                // Requiere el login inicial (Device Code Flow) para poblar el token cache en config.TokenCachePath.
                throw new InvalidOperationException("No hay una sesion de correo autenticada. Es necesario rehacer el login de la cuenta de envio.");
            }

            var resultado = await _app.AcquireTokenSilent(_scopes, cuenta).ExecuteAsync();

            var authProvider = new BaseBearerTokenAuthenticationProvider(new StaticTokenProvider(resultado.AccessToken));
            var graphClient = new GraphServiceClient(authProvider);

            var mensaje = new Message
            {
                Subject = detalle.Asunto,
                Body = new ItemBody
                {
                    ContentType = BodyType.Html,
                    Content = detalle.CuerpoHtml
                },
                ToRecipients =
                [
                    new Recipient { EmailAddress = new EmailAddress { Address = correoDestino } }
                ],
                Attachments = detalle.Adjuntos.Select(a => new FileAttachment
                {
                    Name = a.Nombre,
                    ContentType = a.ContentType,
                    ContentBytes = a.ContenidoBytes
                } as Attachment).ToList()
            };

            await graphClient.Me.SendMail.PostAsync(new SendMailPostRequestBody
            {
                Message = mensaje,
                SaveToSentItems = true
            });
        }

    }

    internal sealed class StaticTokenProvider(string accessToken) : IAccessTokenProvider
    {
        public AllowedHostsValidator AllowedHostsValidator { get; } = new();

        public Task<string> GetAuthorizationTokenAsync(Uri uri, Dictionary<string, object>? additionalAuthenticationContext = null, CancellationToken cancellationToken = default)
            => Task.FromResult(accessToken);
    }
}
