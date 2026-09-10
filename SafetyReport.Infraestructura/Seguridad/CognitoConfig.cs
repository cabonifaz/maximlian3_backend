namespace SafetyReport.Infrastructure.Seguridad;

public class CognitoConfig
{
    public string UserPoolId { get; set; } = string.Empty;
    public string ClientIdFrontend { get; set; } = string.Empty;
    public string ClientIdBackend { get; set; } = string.Empty;
    public string ClientIdN8n { get; set; } = string.Empty;
}
