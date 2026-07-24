namespace SafetyReport.Models
{
    public class EmailConfig
    {
        public string ClientId { get; set; } = string.Empty;
        public string Tenant { get; set; } = "consumers";
        public string TokenCachePath { get; set; } = string.Empty;
    }

    public class PrefacturaEmailDetalle
    {
        public string CodigoPedido { get; set; } = string.Empty;
        public string? NombreInvestigado { get; set; }
        public decimal Costo { get; set; }
    }
}
