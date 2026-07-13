namespace SafetyReport.Models
{
    public class DirectorioEjecutivoCrear
    {
        public int? IdTipoPersona { get; set; }
        public string? NombreCompleto { get; set; }
        public int? IdPais { get; set; }
        public string? Direccion { get; set; }
        public string? Ubigeo { get; set; }
        public string? CodigoPostal { get; set; }
        public int? IdTipoDocumento { get; set; }
        public string? NumeroDocumento { get; set; }
        public int? TaxIdType { get; set; }
        public string? TaxNum { get; set; }
        public int? IdNacionalidad { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public int? IdEstadoCivil { get; set; }
        public int? IdProfesion { get; set; }
        public string? Referencias { get; set; }
    }

    public class DirectorioEjecutivoEditar : DirectorioEjecutivoCrear
    {
        public int IdDirectorioEjecutivo { get; set; }
    }

    public class DirectorioEjecutivoCreado
    {
        public int IdDirectorioEjecutivo { get; set; }
    }

    public class DirectorioEjecutivoEliminado
    {
        public int IdDirectorioEjecutivo { get; set; }
    }

    public class DirectorioEjecutivoIdRequest
    {
        public int IdDirectorioEjecutivo { get; set; }
    }

    public class DirectorioEjecutivoObtenerRequest
    {
        public int? IdDirectorioEjecutivo { get; set; }
        public string? NombreCompleto { get; set; }
        public string? NumeroDocumento { get; set; }
    }

    public class DirectorioEjecutivoConsulta
    {
        public int IdDirectorioEjecutivo { get; set; }
        public int? IdTipoPersona { get; set; }
        public string? TipoPersona { get; set; }
        public string? NombreCompleto { get; set; }
        public int? IdPais { get; set; }
        public string? Pais { get; set; }
        public string? Direccion { get; set; }
        public string? Ubigeo { get; set; }
        public string? CodigoPostal { get; set; }
        public int? IdTipoDocumento { get; set; }
        public string? TipoDocumento { get; set; }
        public string? NumeroDocumento { get; set; }
        public int? TaxIdType { get; set; }
        public string? TaxNum { get; set; }
        public int? IdNacionalidad { get; set; }
        public string? Nacionalidad { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public int? IdEstadoCivil { get; set; }
        public string? EstadoCivil { get; set; }
        public int? IdProfesion { get; set; }
        public string? Profesion { get; set; }
        public string? Referencias { get; set; }
    }

    public class FiltroDirectorioEjecutivo
    {
        public string? Busqueda { get; set; }
        public int NumPag { get; set; } = 1;
    }

    public class DirectorioEjecutivoListaResult
    {
        public List<DirectorioEjecutivoConsulta> lstDirectoriosEjecutivos { get; set; } = new();
        public int TotalRegistros { get; set; }
        public int TotalPaginas { get; set; }
    }
}
