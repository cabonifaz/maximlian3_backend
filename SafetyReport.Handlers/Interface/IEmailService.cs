using SafetyReport.Models;

namespace SafetyReport.Handlers
{
    public interface IEmailService
    {
        Task EnviarPrefacturaAsync(string correoDestino, PrefacturaEmailDetalle detalle);
    }
}
