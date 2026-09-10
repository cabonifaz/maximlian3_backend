namespace SafetyReport.Infrastructure.Correo
{
    public class EmailConfig
    {
        public string ClientId { get; set; } = string.Empty;
        public string Tenant { get; set; } = "consumers";
        public string TokenCachePath { get; set; } = string.Empty;
    }
}
