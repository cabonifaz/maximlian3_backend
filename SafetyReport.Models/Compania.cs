using System.Text.Json;

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
        public string? Direccion { get; set; }
        public string? CiudadProvinciaEstado { get; set; }
        public string? CodigoPostal { get; set; }
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
        public string? Direccion { get; set; }
        public string? CiudadProvinciaEstado { get; set; }
        public string? CodigoPostal { get; set; }
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
        public string? Direccion { get; set; }
        public string? CiudadProvinciaEstado { get; set; }
        public string? CodigoPostal { get; set; }
        public bool? ExisteInformacion { get; set; }
    }

    public class CompaniaListaConsulta
    {
        public int IdCompania { get; set; }
        public string? TipoDocumento { get; set; }
        public string? NumeroDocumento { get; set; }
        public string? NombreCompleto { get; set; }
        public string? Pais { get; set; }
        public string? Telefono { get; set; }
        public string? ExisteInformacion { get; set; }
    }

    public class FiltroCompania
    {
        public string? Busqueda { get; set; }
        public int NumPag { get; set; } = 1;
    }

    public class FiltroCompaniaBusqueda
    {
        public string? Busqueda { get; set; }
        public int? IdPais { get; set; }
    }

    public class CompaniaBusquedaItem
    {
        public int IdCompania { get; set; }
        public string? NumeroDocumento { get; set; }
        public string? NombreCompleto { get; set; }
        public string? NombreComercial { get; set; }
        public string? TipoPersona { get; set; }
    }

    public class CompaniaListaResult
    {
        public List<CompaniaListaConsulta> lstCompanias { get; set; } = new();
        public int TotalRegistros { get; set; }
        public int TotalPaginas { get; set; }
    }

    public class CompaniaMatchItem
    {
        public string? NombreCompleto { get; set; }
    }

    public class CompaniaMatchResultItem
    {
        public int? IdCompania { get; set; }
    }

    public class CompaniaNoticiaArchivoItem
    {
        public int? IdCompaniaNoticiaArchivo { get; set; }
        public int IdTipoArchivo { get; set; }
        public string? NombreArchivo { get; set; }
        public string? NombreDocumento { get; set; }
        public string? FormatoArchivo { get; set; }
        public string? ArchivoUrl { get; set; }
        public string? UploadUrl { get; set; }
    }

    public class CompaniaNoticiaCrear
    {
        public int IdCompania { get; set; }
        public string? Titulo { get; set; }
        public string? Descripcion { get; set; }
        public DateTime? FechaNoticia { get; set; }
        public string? Categoria { get; set; }
        public List<CompaniaNoticiaArchivoItem> Archivos { get; set; } = new();
    }

    public class CompaniaNoticiaEditar
    {
        public int IdCompaniaNoticia { get; set; }
        public int IdCompania { get; set; }
        public string? Titulo { get; set; }
        public string? Descripcion { get; set; }
        public DateTime? FechaNoticia { get; set; }
        public string? Categoria { get; set; }
        public List<CompaniaNoticiaArchivoItem> Archivos { get; set; } = new();
    }

    public class CompaniaNoticiaCreada
    {
        public int IdCompaniaNoticia { get; set; }
        public List<CompaniaNoticiaArchivoItem> Archivos { get; set; } = new();
    }

    public class CompaniaNoticiaEliminada
    {
        public int IdCompaniaNoticia { get; set; }
    }

    public class CompaniaNoticiaIdRequest
    {
        public int IdCompaniaNoticia { get; set; }
    }

    public class CompaniaNoticiaObtenerRequest
    {
        public int? IdCompaniaNoticia { get; set; }
        public int? IdCompania { get; set; }
    }

    public class CompaniaNoticiaArchivoConsulta
    {
        public int IdCompaniaNoticiaArchivo { get; set; }
        public int IdCompaniaNoticia { get; set; }
        public int IdTipoArchivo { get; set; }
        public string? TipoArchivo { get; set; }
        public string? NombreDocumento { get; set; }
    }

    public class CompaniaNoticiaArchivoIdRequest
    {
        public int IdCompaniaNoticiaArchivo { get; set; }
    }

    public class CompaniaNoticiaArchivoDescargaConsulta
    {
        public int IdCompaniaNoticiaArchivo { get; set; }
        public int IdCompaniaNoticia { get; set; }
        public int IdTipoArchivo { get; set; }
        public string? ArchivoUrl { get; set; }
        public string? NombreDocumento { get; set; }
        public string? DownloadUrl { get; set; }
    }

    public class CompaniaNoticiaArchivoEliminado
    {
        public int IdCompaniaNoticiaArchivo { get; set; }
        public int IdCompaniaNoticia { get; set; }
        public string? ArchivoUrl { get; set; }
    }

    public class CompaniaNoticiaConsulta
    {
        public int IdCompaniaNoticia { get; set; }
        public int IdCompania { get; set; }
        public string? Titulo { get; set; }
        public string? Descripcion { get; set; }
        public string? FechaNoticia { get; set; }
        public string? Categoria { get; set; }
        public List<CompaniaNoticiaArchivoConsulta> Archivos { get; set; } = new();
    }

    public class CompaniaNoticiaListaConsulta
    {
        public int IdCompaniaNoticia { get; set; }
        public int IdCompania { get; set; }
        public string? NombreCompleto { get; set; }
        public string? Titulo { get; set; }
        public string? Descripcion { get; set; }
        public string? FechaNoticia { get; set; }
        public string? Categoria { get; set; }
    }

    public class FiltroCompaniaNoticia
    {
        public int? IdCompania { get; set; }
        public string? Busqueda { get; set; }
        public DateTime? FchInicio { get; set; }
        public DateTime? FchFin { get; set; }
        public int NumPag { get; set; } = 1;
    }

    public class CompaniaNoticiaListaResult
    {
        public List<CompaniaNoticiaListaConsulta> lstCompaniaNoticias { get; set; } = new();
        public int TotalRegistros { get; set; }
        public int TotalPaginas { get; set; }
    }

    public class FiltroCompaniaNoticiaBalance
    {
        public int? IdCompania { get; set; }
        public string? Busqueda { get; set; }
        public string? TipoEstadoFinanciero { get; set; }
        public string? Estado { get; set; }
        public DateTime? FchInicio { get; set; }
        public DateTime? FchFin { get; set; }
        public int NumPag { get; set; } = 1;
    }

    public class CompaniaNoticiaBalanceListaConsulta
    {
        public int IdInformeBalance { get; set; }
        public int IdCompania { get; set; }
        public string? NombreCompleto { get; set; }
        public string? FechaInicio { get; set; }
        public string? FechaFin { get; set; }
        public string? Pais { get; set; }
        public string? TipoEstadoFinanciero { get; set; }
        public string? Estado { get; set; }
    }

    public class CompaniaNoticiaBalanceListaResult
    {
        public List<CompaniaNoticiaBalanceListaConsulta> lstCompaniaNoticiasBalance { get; set; } = new();
        public int TotalRegistros { get; set; }
        public int TotalPaginas { get; set; }
    }

    public class CompaniaNoticiaBalanceObtenerRequest
    {
        public int? IdInformeBalance { get; set; }
        public int? IdCompania { get; set; }
    }

    public class CompaniaNoticiaBalanceConsulta
    {
        public int IdInformeBalance { get; set; }
        public int IdCompania { get; set; }
        public string? NombreCompleto { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string? Pais { get; set; }
        public int? IdTipoEstadoFinanciero { get; set; }
        public string? TipoEstadoFinanciero { get; set; }
        public JsonElement? DetalleBalance { get; set; }
    }

    public class FiltroCompaniaNoticiaDetalle
    {
        public int? IdCompania { get; set; }
        public string? Busqueda { get; set; }
        public string? Paises { get; set; }
        public string? Actividades { get; set; }
        public int NumPag { get; set; } = 1;
    }

    public class CompaniaNoticiaDetalleListaConsulta
    {
        public int IdCompania { get; set; }
        public string? NombreCompleto { get; set; }
        public string? NumeroDocumento { get; set; }
        public string? Pais { get; set; }
        public string? Bandera { get; set; }
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }
        public string? ActividadComercial { get; set; }
    }

    public class CompaniaNoticiaDetalleListaResult
    {
        public List<CompaniaNoticiaDetalleListaConsulta> lstCompaniaNoticiasDetalle { get; set; } = new();
        public int TotalRegistros { get; set; }
        public int TotalPaginas { get; set; }
    }

    public class CompaniaNoticiaDetalleExportacion
    {
        public string NombreArchivo { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public byte[] Archivo { get; set; } = [];
    }
}
