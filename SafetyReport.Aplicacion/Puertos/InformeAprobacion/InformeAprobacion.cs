namespace SafetyReport.Application.Puertos.InformeAprobacion;

public class FiltroInformeAprobacionPendientes
{
    public int? NumPag { get; set; }
}

public class InformeAprobacionPendienteConsulta
{
    public int IdInforme { get; set; }
    public string? Investigado { get; set; }
    public string? Pais { get; set; }
    public string? Plantilla { get; set; }
    public string? Idioma { get; set; }
    public string? Usuario { get; set; }
    public string? Fecha { get; set; }
}

public class InformeAprobacionPendientesListaResult
{
    public List<InformeAprobacionPendienteConsulta> lstInformes { get; set; } = new();
    public int TotalRegistros { get; set; }
    public int TotalPaginas { get; set; }
}

public class InformeAprobacionAprobarRequest
{
    public int IdInforme { get; set; }
}
