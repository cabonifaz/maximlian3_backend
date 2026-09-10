namespace SafetyReport.Application.Puertos.Informe
{
    public class InformeUrlPrefirmadaRequest
    {
        public string FileName { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
    }

    public class InformeAutocompletar
    {
        public string FileKey { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
        public object? Secciones { get; set; }
        public string? Prompt { get; set; }
    }

    public class InformeUrlPrefirmada
    {
        public string UploadUrl { get; set; } = string.Empty;
        public string FileKey { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
    }
}
