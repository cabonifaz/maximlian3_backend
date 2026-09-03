namespace SafetyReport.Models
{
    public class EmailConfig
    {
        public string ClientId { get; set; } = string.Empty;
        public string Tenant { get; set; } = "consumers";
        public string TokenCachePath { get; set; } = string.Empty;
    }

    public class NotificacionInformeEmailDetalle
    {
        public string CodigoPedido { get; set; } = string.Empty;
        public string Asunto { get; set; } = string.Empty;
        public string CuerpoHtml { get; set; } = string.Empty;
        public List<EmailAdjunto> Adjuntos { get; set; } = new();
    }

    public class EmailAdjunto
    {
        public string Nombre { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public byte[] ContenidoBytes { get; set; } = [];
    }
}
