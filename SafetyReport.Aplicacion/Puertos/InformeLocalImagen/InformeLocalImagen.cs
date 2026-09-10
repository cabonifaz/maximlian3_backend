namespace SafetyReport.Application.Puertos.InformeLocalImagen;

public class InformeLocalImagenItem
{
    public int? IdInformeLocalImagen { get; set; }
    public int IdTipoArchivo { get; set; }
    public string? Nombre { get; set; }
    public string ImagenURL { get; set; } = string.Empty;
}

public class InformeLocalImagenPendiente
{
    public int IdInformeLocalImagen { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string UploadUrl { get; set; } = string.Empty;
    [System.Text.Json.Serialization.JsonIgnore]
    public string S3Key { get; set; } = string.Empty;
}

public class InformeLocalImagenEstadoCargaRequest
{
    public List<int> Ids { get; set; } = new();
}

public class InformeLocalImagenUrl
{
    public int IdInformeLocalImagen { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string ImagenURL { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
}

public class InformeLocalImagenConsulta
{
    public int IdInformeLocalImagen { get; set; }
    public string ImagenURL { get; set; } = string.Empty;
    public int IdTipoArchivo { get; set; }
    public string? Nombre { get; set; }
}
