namespace SafetyReport.Application.Puertos.InformeObservacion;

public class InformeObservacionItem
{
    public string? Observacion { get; set; }
    public bool Checked { get; set; }
}

public class InformeObservacionInsertarRequest
{
    public int IdInforme { get; set; }
    public int IdPedido { get; set; }
    public List<InformeObservacionItem> Observaciones { get; set; } = new();
}

public class InformeObservacionEditarRequest
{
    public int IdInformeObservacion { get; set; }
    public string? Observacion { get; set; }
    public bool Checked { get; set; }
}

public class InformeObservacionListarRequest
{
    public int IdPedido { get; set; }
}

public class InformeObservacionConsulta
{
    public int IdInformeObservacion { get; set; }
    public int IdInforme { get; set; }
    public int IdPedido { get; set; }
    public string? Observacion { get; set; }
    public bool Checked { get; set; }
}

public class InformeObservacionIdRequest
{
    public int IdInformeObservacion { get; set; }
}

public class InformeObservacionIdResult
{
    public int IdInformeObservacion { get; set; }
}
