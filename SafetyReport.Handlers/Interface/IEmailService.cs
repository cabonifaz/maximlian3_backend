using SafetyReport.Models;

namespace SafetyReport.Handlers
{
    public interface IEmailService
    {
        Task EnviarNotificacionInformeAsync(string correoDestino, NotificacionInformeEmailDetalle detalle);
    }
}
