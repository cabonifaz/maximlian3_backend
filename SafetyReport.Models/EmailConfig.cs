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
        public bool EsIngles { get; set; }
        public string CodigoPedido { get; set; } = string.Empty;
        public string? NombreInvestigado { get; set; }
        public string? Pais { get; set; }
        public string? Moneda { get; set; }
        public string? Tramite { get; set; }
        public string? DiasMinMax { get; set; }
        public decimal Costo { get; set; }
        public decimal Penalidad { get; set; }
    }
}
