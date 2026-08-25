namespace SafetyReport.Models
{
    public class FiltroFacturacionAnaliticaRequest
    {
        public DateOnly? fechaDesde { get; set; }
        public DateOnly? fechaHasta { get; set; }
        public int? idCliente { get; set; }
        public int? idPais { get; set; }
        public int? idTipoTramite { get; set; }
        // 1=Aceptada, 2=Rechazada, 3=Borrador, 4=Anulada, 5=Dada de baja, 6=En proceso (numeración
        // propia del SP, agrupa TABLA_MAESTRA IdMaestro=77 de ms-facturación — no es una columna real).
        public int? idEstadoBucket { get; set; }
        // 1=Factura, 3=Boleta de venta, 7=Nota de crédito, 8=Nota de débito (TABLA_MAESTRA IdMaestro=6
        // de maximilian_facturacion_staging — mismo código que IdTipoDocumentoMaestro).
        public int? idTipoDocumentoMaestro { get; set; }
    }

    public class EvolucionFacturacionRequest
    {
        public DateOnly? fechaDesde { get; set; }
        public DateOnly? fechaHasta { get; set; }
        public int? idCliente { get; set; }
        public int? idPais { get; set; }
        public int? idTipoTramite { get; set; }
        public int granularidad { get; set; } // 1=Dia, 2=Semana, 3=Mes, 4=Ano
    }

    // Resultado de SP_Facturacion_ResumenAnalitico (result set 2 de 5).
    public class IndicadoresFacturacionConsulta
    {
        public int CantidadPedidosPendientes { get; set; }
        public decimal MontoPendienteFacturar { get; set; }
        public int CantidadPedidosFacturados { get; set; }
        public decimal TotalFacturado { get; set; }
        public decimal TotalNotasCredito { get; set; }
        public decimal TotalNotasDebito { get; set; }
    }

    // Result set 3 — monto aproximado (ValorUnitario*Cantidad-Descuento local, sin IGV real de
    // ms-facturación). No suma exacto contra Indicadores.TotalFacturado, ver comentario del SP.
    public class DesgloseTramiteConsulta
    {
        public int? IdTipoTramite { get; set; }
        public string TipoTramite { get; set; } = string.Empty;
        public int CantidadPedidos { get; set; }
        public decimal MontoFacturado { get; set; }
    }

    // Result set 4 — mismo criterio de monto aproximado que DesgloseTramiteConsulta.
    public class DesglosePaisConsulta
    {
        public int? IdPais { get; set; }
        public string Pais { get; set; } = string.Empty;
        public int CantidadPedidos { get; set; }
        public decimal MontoFacturado { get; set; }
    }

    // Result set 5 — monto exacto (viene de ms-facturación, TotalImporte real con IGV), incluye Notas.
    public class DesgloseEstadoConsulta
    {
        public int IdEstadoBucket { get; set; }
        public string EstadoBucket { get; set; } = string.Empty;
        public int CantidadFacturas { get; set; }
        public decimal MontoFacturado { get; set; }
    }

    public class ResumenAnaliticoFacturacionConsulta
    {
        public IndicadoresFacturacionConsulta Indicadores { get; set; } = new();
        public List<DesgloseTramiteConsulta> DesglosePorTramite { get; set; } = new();
        public List<DesglosePaisConsulta> DesglosePorPais { get; set; } = new();
        public List<DesgloseEstadoConsulta> DesglosePorEstado { get; set; } = new();
    }

    // Resultado de SP_Facturacion_EvolucionAnalitica (result set 2 de 2). Periodo es la clave cruda
    // que devuelve el SP ("2026-01-08" | "2026-W03" | "2026-01" | "2026"); Etiqueta se arma acá en el
    // backend (PedidoFacturaHandler) según granularidad — el frontend no formatea fechas.
    public class EvolucionFacturacionConsulta
    {
        public string Periodo { get; set; } = string.Empty;
        public string Etiqueta { get; set; } = string.Empty;
        public int CantidadPedidos { get; set; }
        public decimal MontoFacturado { get; set; }
    }

    // Resultado de SP_Facturacion_ResumenClientesGlobal (result set 2 de 2) — totales globales, sin
    // filtros (ver comentario en el SP y en facturacion.md: "no varían con los filtros de esta sección").
    public class ResumenClienteGlobalConsulta
    {
        public int IdCliente { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public int CantidadPedidosFacturados { get; set; }
        public decimal TotalFacturado { get; set; }
        public decimal MontoPendienteFacturar { get; set; }
    }
}
