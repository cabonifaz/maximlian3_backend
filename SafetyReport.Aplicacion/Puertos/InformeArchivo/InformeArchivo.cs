namespace SafetyReport.Application.Puertos.InformeArchivo;

public class InformeArchivoItem
{
    public string Nombre { get; set; } = string.Empty;
    public string ArchivoUrl { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public long TamanoBytes { get; set; }
    public int IdTipoArchivo { get; set; }
    public int? IdFaseEvidencia { get; set; }
}

public class InformeArchivoInsertarRequest
{
    public int IdInforme { get; set; }
    public int IdPedido { get; set; }
    public List<InformeArchivoItem> Archivos { get; set; } = new();
}

public class InformeArchivoActualizarRequest
{
    public int IdInformeArchivo { get; set; }
    public int? IdTipoArchivo { get; set; }
    public int? IdFaseEvidencia { get; set; }
}

public class InformeArchivoIdRequest
{
    public int IdInformeArchivo { get; set; }
}

public class InformeArchivoConsulta
{
    public int IdInformeArchivo { get; set; }
    public int IdInforme { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string ArchivoUrl { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public long TamanoBytes { get; set; }
    public int IdTipoArchivo { get; set; }
    public int IdFaseEvidencia { get; set; }
    public string DownloadUrl { get; set; } = string.Empty;
}

public class InformeArchivoResumen
{
    public int IdInformeArchivo { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public long TamanoBytes { get; set; }
    public int IdTipoArchivo { get; set; }
    public int? IdFaseEvidencia { get; set; }
}

public class InformeArchivoUrlRequest
{
    public int IdPedido { get; set; }
    public int IdInforme { get; set; }
    public List<string> Nombres { get; set; } = new();
}

public class InformeArchivoPendiente
{
    public string Nombre { get; set; } = string.Empty;
    public string ArchivoUrl { get; set; } = string.Empty;
    public string UploadUrl { get; set; } = string.Empty;
}

public class InformeArchivoUrlResult
{
    public int IdInforme { get; set; }
    public List<InformeArchivoPendiente> Archivos { get; set; } = new();
}
