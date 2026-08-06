namespace SafetyReport.Models
{
    public class FacturacionElectronicaConfig
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;

        // Base del front donde vive la página pública de verificación — el link completo es
        // {VerificacionFrontendUrl}/{token}. No es la URL de ms-facturación, es la del front (SPA).
        public string VerificacionFrontendUrl { get; set; } = string.Empty;
    }
}
