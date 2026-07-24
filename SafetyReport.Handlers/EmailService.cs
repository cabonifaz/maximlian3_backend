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

        public async Task EnviarPrefacturaAsync(string correoDestino, PrefacturaEmailDetalle detalle)
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
                Subject = detalle.EsIngles
                    ? $"Pro Forma Invoice for Order {detalle.CodigoPedido} - Pending Approval"
                    : $"Prefactura del Pedido {detalle.CodigoPedido} - Pendiente de aprobación",
                Body = new ItemBody
                {
                    ContentType = BodyType.Html,
                    Content = detalle.EsIngles ? ArmarCuerpoHtmlIngles(detalle) : ArmarCuerpoHtmlEspanol(detalle)
                },
                ToRecipients =
                [
                    new Recipient { EmailAddress = new EmailAddress { Address = correoDestino } }
                ]
            };

            await graphClient.Me.SendMail.PostAsync(new SendMailPostRequestBody
            {
                Message = mensaje,
                SaveToSentItems = true
            });

            _logger.LogInformation("Correo de prefactura enviado para el pedido {CodigoPedido} a {Correo}.", detalle.CodigoPedido, correoDestino);
        }

        private static string ArmarCuerpoHtmlEspanol(PrefacturaEmailDetalle detalle) =>
            $"""
            <p>Estimado(a) cliente,</p>
            <p>Le hacemos llegar la prefactura correspondiente al pedido <strong>{detalle.CodigoPedido}</strong> para su revisión y aprobación.</p>
            <table cellpadding="6" cellspacing="0" style="border-collapse: collapse; margin: 12px 0;">
                <tr>
                    <td style="border: 1px solid #ccc; font-weight: bold;">Pedido</td>
                    <td style="border: 1px solid #ccc;">{detalle.CodigoPedido}</td>
                </tr>
                <tr>
                    <td style="border: 1px solid #ccc; font-weight: bold;">Investigado</td>
                    <td style="border: 1px solid #ccc;">{detalle.NombreInvestigado}</td>
                </tr>
                <tr>
                    <td style="border: 1px solid #ccc; font-weight: bold;">País</td>
                    <td style="border: 1px solid #ccc;">{detalle.Pais}</td>
                </tr>
                <tr>
                    <td style="border: 1px solid #ccc; font-weight: bold;">Moneda</td>
                    <td style="border: 1px solid #ccc;">{detalle.Moneda}</td>
                </tr>
                <tr>
                    <td style="border: 1px solid #ccc; font-weight: bold;">Trámite</td>
                    <td style="border: 1px solid #ccc;">{detalle.Tramite}</td>
                </tr>
                <tr>
                    <td style="border: 1px solid #ccc; font-weight: bold;">Días (mín - máx)</td>
                    <td style="border: 1px solid #ccc;">{detalle.DiasMinMax}</td>
                </tr>
                <tr>
                    <td style="border: 1px solid #ccc; font-weight: bold;">Precio</td>
                    <td style="border: 1px solid #ccc;">{detalle.Costo:N2}</td>
                </tr>
                <tr>
                    <td style="border: 1px solid #ccc; font-weight: bold;">Penalidad</td>
                    <td style="border: 1px solid #ccc;">{detalle.Penalidad:N2}</td>
                </tr>
            </table>
            <p>Por favor, confírmenos su <strong>aprobación respondiendo este correo</strong> a la brevedad para continuar con el proceso de facturación.</p>
            <p>Quedamos atentos ante cualquier consulta.</p>
            <p>Saludos cordiales.</p>
            """;

        private static string ArmarCuerpoHtmlIngles(PrefacturaEmailDetalle detalle) =>
            $"""
            <p>Dear Customer,</p>
            <p>Please find below the pro forma invoice for order <strong>{detalle.CodigoPedido}</strong> for your review and approval.</p>
            <table cellpadding="6" cellspacing="0" style="border-collapse: collapse; margin: 12px 0;">
                <tr>
                    <td style="border: 1px solid #ccc; font-weight: bold;">Order</td>
                    <td style="border: 1px solid #ccc;">{detalle.CodigoPedido}</td>
                </tr>
                <tr>
                    <td style="border: 1px solid #ccc; font-weight: bold;">Subject</td>
                    <td style="border: 1px solid #ccc;">{detalle.NombreInvestigado}</td>
                </tr>
                <tr>
                    <td style="border: 1px solid #ccc; font-weight: bold;">Country</td>
                    <td style="border: 1px solid #ccc;">{detalle.Pais}</td>
                </tr>
                <tr>
                    <td style="border: 1px solid #ccc; font-weight: bold;">Currency</td>
                    <td style="border: 1px solid #ccc;">{detalle.Moneda}</td>
                </tr>
                <tr>
                    <td style="border: 1px solid #ccc; font-weight: bold;">Service Type</td>
                    <td style="border: 1px solid #ccc;">{detalle.Tramite}</td>
                </tr>
                <tr>
                    <td style="border: 1px solid #ccc; font-weight: bold;">Days (min - max)</td>
                    <td style="border: 1px solid #ccc;">{detalle.DiasMinMax}</td>
                </tr>
                <tr>
                    <td style="border: 1px solid #ccc; font-weight: bold;">Price</td>
                    <td style="border: 1px solid #ccc;">{detalle.Costo:N2}</td>
                </tr>
                <tr>
                    <td style="border: 1px solid #ccc; font-weight: bold;">Penalty</td>
                    <td style="border: 1px solid #ccc;">{detalle.Penalidad:N2}</td>
                </tr>
            </table>
            <p>Please confirm your <strong>approval by replying to this email</strong> as soon as possible so we can proceed with the invoicing process.</p>
            <p>Please let us know if you have any questions.</p>
            <p>Best regards.</p>
            """;
    }

    internal sealed class StaticTokenProvider(string accessToken) : IAccessTokenProvider
    {
        public AllowedHostsValidator AllowedHostsValidator { get; } = new();

        public Task<string> GetAuthorizationTokenAsync(Uri uri, Dictionary<string, object>? additionalAuthenticationContext = null, CancellationToken cancellationToken = default)
            => Task.FromResult(accessToken);
    }
}
