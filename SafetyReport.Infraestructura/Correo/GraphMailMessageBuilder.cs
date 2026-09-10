using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions.Authentication;
using SafetyReport.Application.Puertos.Informe;

namespace SafetyReport.Infrastructure.Correo
{
    internal static class GraphMailMessageBuilder
    {
        public static Message Construir(string correoDestino, NotificacionInformeEmailDetalle detalle)
        {
            return new Message
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
        }
    }

    internal sealed class StaticTokenProvider(string accessToken) : IAccessTokenProvider
    {
        public AllowedHostsValidator AllowedHostsValidator { get; } = new();

        public Task<string> GetAuthorizationTokenAsync(Uri uri, Dictionary<string, object>? additionalAuthenticationContext = null, CancellationToken cancellationToken = default)
            => Task.FromResult(accessToken);
    }
}
