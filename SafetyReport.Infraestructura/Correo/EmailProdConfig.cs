namespace SafetyReport.Infrastructure.Correo
{
    public class EmailProdConfig
    {
        public string TenantId { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string SenderMailbox { get; set; } = string.Empty;
    }
}
