namespace SafetyReport.Domain.Informes;

public static class InformeDocumentoReglas
{
    public static string? MapearExtensionFormato(string formatoLabel)
    {
        return formatoLabel switch
        {
            "Word" => ".docx",
            "PDF" => ".pdf",
            "XML" => ".xml",
            _ => null
        };
    }

    public static string MapearContentType(string formato)
    {
        return formato switch
        {
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".pdf" => "application/pdf",
            ".xml" => "application/xml",
            _ => "application/octet-stream"
        };
    }
}
