using Microsoft.Graph;
using Microsoft.Graph.Users.Item.SendMail;
using Microsoft.Identity.Client;
using Microsoft.Kiota.Abstractions.Authentication;
using SafetyReport.Application.Puertos.Informe;

namespace SafetyReport.Infrastructure.Correo
{
    // Envio via Graph con auth app-only (client credentials), usado en Production
    // contra el buzon organizacional real. No requiere login interactivo ni token
    // cache persistido: MSAL adquiere un token nuevo en cada envio con ClientId/Secret.
    public class EmailServiceProd : IInformeEmailSender
    {
        private static readonly string[] _scopes = ["https://graph.microsoft.com/.default"];

        private readonly IConfidentialClientApplication _app;
        private readonly string _senderMailbox;

        public EmailServiceProd(EmailProdConfig config)
        {
            _senderMailbox = config.SenderMailbox;

            _app = ConfidentialClientApplicationBuilder
                .Create(config.ClientId)
                .WithClientSecret(config.ClientSecret)
                .WithAuthority($"https://login.microsoftonline.com/{config.TenantId}")
                .Build();
        }

        public async Task EnviarNotificacionInformeAsync(string correoDestino, NotificacionInformeEmailDetalle detalle)
        {
            var resultado = await _app.AcquireTokenForClient(_scopes).ExecuteAsync();

            var authProvider = new BaseBearerTokenAuthenticationProvider(new StaticTokenProvider(resultado.AccessToken));
            var graphClient = new GraphServiceClient(authProvider);

            var mensaje = GraphMailMessageBuilder.Construir(correoDestino, detalle);

            await graphClient.Users[_senderMailbox].SendMail.PostAsync(new SendMailPostRequestBody
            {
                Message = mensaje,
                SaveToSentItems = true
            });
        }
    }
}
