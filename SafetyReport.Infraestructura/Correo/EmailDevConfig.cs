namespace SafetyReport.Infrastructure.Correo
{
    public class EmailDevConfig
    {
        public string ClientId { get; set; } = string.Empty;
        public string Tenant { get; set; } = "consumers";
        public string TokenCacheS3Key { get; set; } = string.Empty;
    }
}
