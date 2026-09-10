using SafetyReport.Application.Puertos.InformeLocalImagen;

namespace SafetyReport.Application.Puertos.Informe
{
    public class InformeLocalItem
    {
        public int? IdInformeLocal { get; set; }
        public int? IdTipoLocal { get; set; }
        public string? Comentario { get; set; }
        public List<InformeLocalImagenItem> Imagenes { get; set; } = new();
    }

    public class InformeBancoItem
    {
        public int? IdInformeBanco { get; set; }
        public int IdBanco { get; set; }
        public string? NumeroCuenta { get; set; }
        public int? IdSector { get; set; }
        public string? Sectorista { get; set; }
        public string? ReferenciaBanco { get; set; }
    }

    public class InformeCompaniaRelacionadaItem
    {
        public int? IdInformeCompaniaRelacionada { get; set; }
        public int IdCompania { get; set; }
    }

    public class InformeExportacionImportacionItem
    {
        public int? IdInformeExportacionImportacion { get; set; }
        public int Anio { get; set; }
        public int MesInicio { get; set; }
        public int MesFin { get; set; }
        public int IdMoneda { get; set; }
        public string? Paises { get; set; }
        public decimal? Monto { get; set; }
        public string? Productos { get; set; }
        public int IdTipoOperacion { get; set; }
        public int? NumOperaciones { get; set; }
    }

    public class InformeProveedorItem
    {
        public int? IdInformeProveedor { get; set; }
        public int? IdBancoProveedor { get; set; }
        public int IdTipoPersona { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int? IdPais { get; set; }
        public int? IdTipoDocumento { get; set; }
        public string? NumeroDocumento { get; set; }
        public int? IdMoneda { get; set; }
        public DateTime? FechaInicio { get; set; }
        public int? IdLimiteCredito { get; set; }
        public decimal? PromedioMensual { get; set; }
        public string? PlazoCredito { get; set; }
        public string? Productos { get; set; }
        public int? IdCalificacion { get; set; }
        public string? Comentarios { get; set; }
        public string? NombreContacto { get; set; }
        public string? Telefono { get; set; }
        public string? ComienzoNegociaciones { get; set; }
        public int? IdPlazoCredito { get; set; }
        public bool? EsTieneReferenciaComercial { get; set; }
        public decimal? TipoCambio { get; set; }
    }

    public class InformeDirectorioEjecutivoItem
    {
        public int? IdInformeDirectorioEjecutivo { get; set; }
        public int IdDirectorioEjecutivo { get; set; }
        public int? IdCargo { get; set; }
        public DateTime? VinculadoDesde { get; set; }
        public string? CompaniaAnterior { get; set; }
        public decimal? Participacion { get; set; }
        public int? Orden { get; set; }
        public bool? EsParticipanteDirectiva { get; set; }
        public bool? ApareceImpresoLista { get; set; }
        public bool? ImprimeDatosEjecutivos { get; set; }
    }
}
