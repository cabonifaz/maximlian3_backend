namespace SafetyReport.Models
{
    public class CompaniaCrear
    {
        public int? IdTipoPersona { get; set; }
        public int? IdTipoDocumento { get; set; }
        public string? NumeroDocumento { get; set; }
        public string? NombreCompleto { get; set; }
        public int? IdPais { get; set; }
        public string? Telefono { get; set; }
        public bool? ExisteInformacion { get; set; }
    }

    public class CompaniaEditar
    {
        public int IdCompania { get; set; }
        public int? IdTipoPersona { get; set; }
        public int? IdTipoDocumento { get; set; }
        public string? NumeroDocumento { get; set; }
        public string? NombreCompleto { get; set; }
        public int? IdPais { get; set; }
        public string? Telefono { get; set; }
        public bool? ExisteInformacion { get; set; }
    }

    public class CompaniaCreada
    {
        public int IdCompania { get; set; }
    }

    public class CompaniaEliminada
    {
        public int IdCompania { get; set; }
    }

    public class CompaniaIdRequest
    {
        public int IdCompania { get; set; }
    }

    public class CompaniaObtenerRequest
    {
        public int? IdCompania { get; set; }
        public string? NumDocumento { get; set; }
        public string? Nombre { get; set; }
    }

    public class CompaniaConsulta
    {
        public int IdCompania { get; set; }
        public int? IdTipoPersona { get; set; }
        public string? TipoPersona { get; set; }
        public int? IdTipoDocumento { get; set; }
        public string? TipoDocumento { get; set; }
        public string? NumeroDocumento { get; set; }
        public string? NombreCompleto { get; set; }
        public int? IdPais { get; set; }
        public string? Pais { get; set; }
        public string? Telefono { get; set; }
        public bool? ExisteInformacion { get; set; }
    }

    public class FiltroCompania
    {
        public string? Busqueda { get; set; }
        public int NumPag { get; set; } = 1;
    }

    public class CompaniaListaResult
    {
        public List<CompaniaConsulta> lstCompanias { get; set; } = new();
        public int TotalRegistros { get; set; }
        public int TotalPaginas { get; set; }
    }

    public class CompaniaMatchItem
    {
        public string? NumeroDocumento { get; set; }
        public string? NombreCompleto { get; set; }
    }

    public class CompaniaMatchResultItem
    {
        public int? IdCompania { get; set; }
    }
}
