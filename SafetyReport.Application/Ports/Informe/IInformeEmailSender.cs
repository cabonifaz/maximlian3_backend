using SafetyReport.Models;

namespace SafetyReport.Application.Ports.Informe;

public interface IInformeEmailSender
{
    Task EnviarNotificacionInformeAsync(string correoDestino, NotificacionInformeEmailDetalle detalle);
}
