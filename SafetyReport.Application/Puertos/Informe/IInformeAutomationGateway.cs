namespace SafetyReport.Application.Puertos.Informe;

public interface IInformeAutomationGateway
{
    Task<string> PostAsync(string webhookUrl, object payload);
}
