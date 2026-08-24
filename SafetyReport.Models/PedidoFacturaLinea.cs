namespace SafetyReport.Models
{
    // Body de POST — SP_PedidoFacturaLinea_Crear. Toda línea nace libre (IdDocumentoElectronico
    // NULL); la asociación a un documento pasa exclusivamente por RegistrarEnvio (ver
    // PLAN_Lineas_Facturacion.md).
    public class CrearLineaFacturacionRequest
    {
        public List<int> idsPedido { get; set; } = new();
        public string? codigo { get; set; }
        public string descripcion { get; set; } = string.Empty;
    }

    // Codigo/Descripcion son la única metadata editable de una línea existente — Cantidad,
    // ValorUnitario, Descuento y la composición de pedidos no se tocan acá.
    public class ActualizarLineaFacturacionRequest
    {
        public string? codigo { get; set; }
        public string descripcion { get; set; } = string.Empty;
    }

    public class PedidoFacturaLineaConsulta
    {
        public int IdPedidoFacturaLinea { get; set; }
        public string? Codigo { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal ValorUnitario { get; set; }
        public decimal Descuento { get; set; }
        public decimal? DescuentoPorcentaje { get; set; }
    }

    // Query params de SP_PedidoFacturaLinea_Listar. idCliente es obligatorio — una línea agrupa
    // pedidos de un único cliente (guarda de SP_PedidoFacturaLinea_Crear), así que listar sin
    // acotar por cliente no tiene un caso de uso hoy. anio/mes son opcionales, mismo criterio de
    // mes único que ListarPedidosFacturacionRequest (ver PLAN_Lineas_Facturacion.md).
    public class ListarLineasFacturacionRequest
    {
        public int idCliente { get; set; }
        public int? anio { get; set; }
        public int? mes { get; set; }
    }

    // Resultado de SP_PedidoFacturaLinea_Listar. IdTipoTramite/TipoTramite/IdMoneda/Moneda se
    // resuelven vía el TARIFARIO de un pedido miembro (todos comparten el mismo IdTarifario).
    public class PedidoFacturaLineaListaConsulta
    {
        public int IdPedidoFacturaLinea { get; set; }
        public string? Codigo { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public int? IdTipoTramite { get; set; }
        public string? TipoTramite { get; set; }
        public int? IdMoneda { get; set; }
        public string? Moneda { get; set; }
        public int Cantidad { get; set; }
        public decimal ValorUnitario { get; set; }
        public decimal Descuento { get; set; }
        public decimal? DescuentoPorcentaje { get; set; }
    }

    public class PedidoFacturaLineaListaResult
    {
        public List<PedidoFacturaLineaListaConsulta> Lineas { get; set; } = new();
    }
}