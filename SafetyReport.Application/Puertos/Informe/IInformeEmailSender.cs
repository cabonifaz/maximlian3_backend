using SafetyReport.Application.Puertos.Informe;

namespace SafetyReport.Application.Puertos.Informe;

public interface IInformeEmailSender
{
    Task EnviarNotificacionInformeAsync(string correoDestino, NotificacionInformeEmailDetalle detalle);
}
