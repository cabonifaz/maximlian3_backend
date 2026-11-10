using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SafetyReport.Application.Puertos.FlujoInforme
{
    // SRC-F1-065: flujo secuencial analista → traductor, repositorio de informes y complementos.
    // Los nombres de las propiedades siguen los tipos del front (src/shared/types/flujo-informe.type.ts).

    public static class EtapaFlujoPedido
    {
        public const string SinAsignar = "sin-asignar";
        public const string EnAnalisis = "en-analisis";
        public const string ListoTraduccion = "listo-traduccion";
        public const string EnTraduccion = "en-traduccion";
        public const string PorRevisar = "por-revisar";
        public const string Finalizado = "finalizado";

        public static readonly string[] Todas = { SinAsignar, EnAnalisis, ListoTraduccion, EnTraduccion, PorRevisar, Finalizado };
    }

    public static class RolFlujoAsignacion
    {
        public const string Analista = "analista";
        public const string Traductor = "traductor";

        public const int IdRolAnalista = 3;
        public const int IdRolTraductor = 4;

        public static int? ObtenerIdRol(string? rol) => rol?.Trim().ToLowerInvariant() switch
        {
            Analista => IdRolAnalista,
            Traductor => IdRolTraductor,
            _ => null
        };
    }

    // ── Asignaciones por etapa ─────────────────────────────────────────────────

    public class FiltroFlujoAsignacion : IValidatableObject
    {
        [Required]
        public string Etapa { get; set; } = string.Empty;
        public string? Busqueda { get; set; }
        public int? NumPag { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!EtapaFlujoPedido.Todas.Contains(Etapa))
            {
                yield return new ValidationResult(
                    $"etapa debe ser una de: {string.Join(", ", EtapaFlujoPedido.Todas)}.",
                    new[] { nameof(Etapa) });
            }
        }
    }

    public class FilaFlujoAsignacion
    {
        public int IdPedido { get; set; }
        public string? Cliente { get; set; }
        public string? Investigado { get; set; }
        public string? Pais { get; set; }
        public int IdIdioma { get; set; }
        public string? IdiomaInforme { get; set; }
        public bool RequiereTraduccion { get; set; }
        public string Prioridad { get; set; } = "normal";
        public DateTime? FechaPedido { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public string Etapa { get; set; } = string.Empty;
        public int? IdUsuarioAnalista { get; set; }
        public string? Analista { get; set; }
        public int? IdUsuarioTraductor { get; set; }
        public string? Traductor { get; set; }
        public int? IdInformeAnalista { get; set; }
        public int? IdInformeTraductor { get; set; }
        public DateTime? FechaFinAnalisis { get; set; }
        public bool Rechazado { get; set; }
        public DateTime? FechaFinalizacion { get; set; }
    }

    // El front indexa los conteos por el id del tab (Record<TabFlujoAsignacion, number>).
    public class ConteoTabsFlujoAsignacion
    {
        [JsonPropertyName("sin-asignar")] public int SinAsignar { get; set; }
        [JsonPropertyName("en-analisis")] public int EnAnalisis { get; set; }
        [JsonPropertyName("listo-traduccion")] public int ListoTraduccion { get; set; }
        [JsonPropertyName("en-traduccion")] public int EnTraduccion { get; set; }
        [JsonPropertyName("por-revisar")] public int PorRevisar { get; set; }
        [JsonPropertyName("complementos")] public int Complementos { get; set; }
        [JsonPropertyName("finalizado")] public int Finalizados { get; set; }
    }

    public class ListaFlujoAsignacion
    {
        public List<FilaFlujoAsignacion> lstPedido { get; set; } = new();
        public int TotalRegistros { get; set; }
        public int TotalPaginas { get; set; }
        public ConteoTabsFlujoAsignacion Conteos { get; set; } = new();
    }

    public class AsignarRolFlujoRequest
    {
        [Range(1, int.MaxValue)]
        public int IdPedido { get; set; }

        [Required]
        public string Rol { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int IdUsuarioAsignado { get; set; }
    }

    public class AsignacionFlujoCreada
    {
        public int IdAsignacion { get; set; }
    }

    // ── Repositorio de informes ────────────────────────────────────────────────

    public class FiltroRepositorioInformes
    {
        public string? Busqueda { get; set; }
        public string? Tipo { get; set; }
        public int? NumPag { get; set; }
    }

    public class InformeRepositorio
    {
        public int IdInforme { get; set; }
        public int IdCompania { get; set; }
        public string Tipo { get; set; } = "informe";
        public string? Investigado { get; set; }
        public string? Pais { get; set; }
        public string? Cliente { get; set; }
        public string? Idioma { get; set; }
        public string? Traductor { get; set; }
        public DateTime? FechaAprobacion { get; set; }
        public int? IdInformeBase { get; set; }
        public bool EsUltimaVersion { get; set; }
    }

    public class ListaRepositorioInformes
    {
        public List<InformeRepositorio> lstInforme { get; set; } = new();
        public int TotalRegistros { get; set; }
        public int TotalPaginas { get; set; }
    }

    // ── Complementos ───────────────────────────────────────────────────────────

    public class FiltroComplementos
    {
        public string? Busqueda { get; set; }
        public bool SoloPropios { get; set; }
        public int? NumPag { get; set; }
    }

    public class Complemento
    {
        public int IdComplemento { get; set; }
        public int IdCompania { get; set; }
        public string? Investigado { get; set; }
        public string? Pais { get; set; }
        public int? IdInformeBase { get; set; }
        public string TipoInformeBase { get; set; } = "informe";
        public string? ClienteBase { get; set; }
        public int IdClienteDestino { get; set; }
        public string? ClienteDestino { get; set; }
        public string? Idioma { get; set; }
        public int? IdUsuarioTraductor { get; set; }
        public string? Traductor { get; set; }
        public string Estado { get; set; } = "borrador";
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        // Informe del traductor del complemento (null mientras no lo guarde por primera vez)
        public int? IdInforme { get; set; }
        public string? Codigo { get; set; }
        public int? IdPlantilla { get; set; }
    }

    public class ListaComplementos
    {
        public List<Complemento> lstComplemento { get; set; } = new();
        public int TotalRegistros { get; set; }
        public int TotalPaginas { get; set; }
    }

    public class CrearComplementoRequest
    {
        [Range(1, int.MaxValue)]
        public int IdCompania { get; set; }

        [Range(1, int.MaxValue)]
        public int IdClienteDestino { get; set; }

        [Range(1, int.MaxValue)]
        public int IdIdioma { get; set; }

        [MaxLength(500)]
        public string? Observacion { get; set; }
    }

    public class ComplementoCreado
    {
        public int IdComplemento { get; set; }
        public string? Codigo { get; set; }
        public int? IdInformeBase { get; set; }
    }

    public class EnviarAprobacionComplementoRequest
    {
        [Range(1, int.MaxValue)]
        public int IdComplemento { get; set; }
    }

    public class ComplementoEnviado
    {
        public int IdInforme { get; set; }
    }

    public class ClienteDestinoComplemento
    {
        public int IdCliente { get; set; }
        public string? Nombre { get; set; }
    }

    public class ListaClientesDestinoComplemento
    {
        public List<ClienteDestinoComplemento> lstCliente { get; set; } = new();
    }
}
