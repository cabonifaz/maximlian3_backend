using Microsoft.Extensions.Logging;
using SafetyReport.Application.Puertos.Informe;

namespace SafetyReport.Infrastructure.Correo
{
    // Se registra en vez de EmailServiceDev/EmailServiceProd cuando falta configuracion
    // (Email:Dev o Email:Prod segun el ambiente). Evita que la app no levante por un
    // proveedor de correo mal configurado; la notificacion simplemente no se envia,
    // dejando constancia en el log.
    public class EmailServiceNoop : IInformeEmailSender
    {
        private readonly ILogger<EmailServiceNoop> _logger;

        public EmailServiceNoop(ILogger<EmailServiceNoop> logger)
        {
            _logger = logger;
        }

        public Task EnviarNotificacionInformeAsync(string correoDestino, NotificacionInformeEmailDetalle detalle)
        {
            _logger.LogWarning(
                "Envio de correo deshabilitado por falta de configuracion. No se envio notificacion a {CorreoDestino}.",
                correoDestino);
            return Task.CompletedTask;
        }
    }
}
