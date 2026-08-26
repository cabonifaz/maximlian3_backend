namespace SafetyReport.Models
{
    public class FiltroFacturacionAnaliticaRequest
    {
        public DateOnly? fechaDesde { get; set; }
        public DateOnly? fechaHasta { get; set; }
        public int? idCliente { get; set; }
        public int? idPais { get; set; }
        public int? idTipoTramite { get; set; }
        // TABLA_MAESTRA IdMaestro=77 (local), Num1 1..19 — mismo código real que IdEstadoMaestro en
        // DOCUMENTOS_ELECTRONICOS. Sin bucket propio: el desglose usa los estados reales (top 5 + Otros).
        public int? idEstadoMaestro { get; set; }
        // 1=Factura, 3=Boleta de venta, 7=Nota de crédito, 8=Nota de débito (TABLA_MAESTRA IdMaestro=6
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

    public class IndicadoresFacturacionConsulta
    {
        public int CantidadPedidosPendientes { get; set; }
        public decimal MontoPendienteFacturar { get; set; }
        public int CantidadPedidosFacturados { get; set; }
        public decimal TotalFacturado { get; set; }
        public decimal TotalNotasCredito { get; set; }
        public decimal TotalNotasDebito { get; set; }
    }

    public class DesgloseTramiteConsulta
    {
        public int? IdTipoTramite { get; set; }
        public string TipoTramite { get; set; } = string.Empty;
        public int CantidadPedidos { get; set; }
        public decimal MontoFacturado { get; set; }
    }

    public class DesglosePaisConsulta
    {
        public int? IdPais { get; set; }
        public string Pais { get; set; } = string.Empty;
        public int CantidadPedidos { get; set; }
        public decimal MontoFacturado { get; set; }
    }

    public class DesgloseEstadoConsulta
    {
        public int? IdEstadoMaestro { get; set; }
        public string Estado { get; set; } = string.Empty;
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

    public class EvolucionFacturacionConsulta
    {
        public string Periodo { get; set; } = string.Empty;
        public string Etiqueta { get; set; } = string.Empty;
        public int CantidadPedidos { get; set; }
        public decimal MontoFacturado { get; set; }
    }

    public class ResumenClienteGlobalConsulta
    {
        public int IdCliente { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public int CantidadPedidosFacturados { get; set; }
        public decimal TotalFacturado { get; set; }
        public decimal MontoPendienteFacturar { get; set; }
    }
}
