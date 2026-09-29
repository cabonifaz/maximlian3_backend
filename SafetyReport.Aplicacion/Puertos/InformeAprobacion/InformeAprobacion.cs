namespace SafetyReport.Application.Puertos.InformeAprobacion;

public class FiltroInformeAprobacionPendientes
{
    public int? IdPais { get; set; }
    public int? IdPlantilla { get; set; }
    public int? NumPag { get; set; }
}

public class InformeAprobacionPendienteConsulta
{
    public int IdInforme { get; set; }
    public int IdPedido { get; set; }
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
    public List<int> IdInformes { get; set; } = new();
}
