namespace SafetyReport.Application.Ports.Informe;

public interface IInformeAutomationGateway
{
    Task<string> PostAsync(string webhookUrl, object payload);
}
