namespace SafetyReport.Application.Puertos.Informe;

public interface IInformeAutomationGateway
{
    Task<string> ObtenerCamposAsync(object payload);
}
