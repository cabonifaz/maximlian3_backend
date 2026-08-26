namespace SafetyReport.Models
{
    // Body de POST — SP_PedidoFacturaLinea_Crear. Toda línea nace libre (IdDocumentoElectronico
    // NULL); la asociación a un documento pasa exclusivamente por RegistrarEnvio (ver
    // PLAN_Lineas_Facturacion.md). idCliente es explícito: el SP valida que todo pedido de
    // idsPedido pertenezca a ese cliente, no solo que sean mutuamente consistentes entre sí.
    public class CrearLineaFacturacionRequest
    {
        public int idCliente { get; set; }
        public List<int> idsPedido { get; set; } = new();
        public string? codigo { get; set; }
        public string descripcion { get; set; } = string.Empty;
    }

    // Body de POST .../lote — SP_PedidoFacturaLinea_CrearLote. Cada elemento de grupos se vuelve
    // una PEDIDO_FACTURA_LINEA: el llamador arma los grupos y manda codigo/descripcion/
    // valorUnitario/descuento por grupo (no autocalculados) — Cantidad la calcula el SP como
    // COUNT(idsPedido).
    public class GrupoLineaLoteRequest
    {
        public List<int> idsPedido { get; set; } = new();
        public string? codigo { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public decimal valorUnitario { get; set; }
        public decimal descuento { get; set; }
    }

    public class CrearLineaFacturacionLoteRequest
    {
        public int idCliente { get; set; }
        public List<GrupoLineaLoteRequest> grupos { get; set; } = new();
    }

    // Codigo/Descripcion son la única metadata editable de una línea existente — Cantidad,
    // ValorUnitario, Descuento y la composición de pedidos no se tocan acá.
    public class ActualizarLineaFacturacionRequest
    {
        public string? codigo { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public decimal valorUnitario { get; set; }
        public decimal descuento { get; set; }
    }

    // Body de PUT .../pedidos — SP_PedidoFacturaLinea_ActualizarPedidos. idsPedido es el conjunto
    // completo deseado (no incremental): todo miembro actual que no venga en la lista se libera,
    // todo pedido nuevo se engancha, y Cantidad/ValorUnitario/Descuento se recalculan desde cero.
    // Si la lista queda vacía, la línea se soft-elimina. Solo aplica si la línea no tiene un
    // estado SUNAT vigente (ver comentario del SP) — no depende de si ya tiene IdDocumentoElectronico.
    public class ActualizarPedidosLineaFacturacionRequest
    {
        public int idCliente { get; set; }
        public List<int> idsPedido { get; set; } = new();
    }

    public class PedidoFacturaLineaConsulta
    {
        public int IdPedidoFacturaLinea { get; set; }
        public string? Codigo { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal ValorUnitario { get; set; }
        public decimal Descuento { get; set; }
    }

    // Query params de SP_PedidoFacturaLinea_Listar. idCliente es obligatorio — una línea agrupa
    // pedidos de un único cliente (guarda de SP_PedidoFacturaLinea_Crear), así que listar sin
    // acotar por cliente no tiene un caso de uso hoy. anio/mes son opcionales, mismo criterio de
    // mes único que ListarPedidosFacturacionRequest (ver PLAN_Lineas_Facturacion.md).
    // idDocumentoElectronico es opcional: además de las líneas libres, también trae las que ya
    // están asociadas a ese documento — para editar un borrador existente sin perder de vista
    // sus líneas actuales.
    public class ListarLineasFacturacionRequest
    {
        public int idCliente { get; set; }
        public int? anio { get; set; }
        public int? mes { get; set; }
        public int? idDocumentoElectronico { get; set; }
        public int? idMoneda { get; set; }
    }

    // Resultado de SP_PedidoFacturaLinea_Listar. IdTipoTramite/TipoTramite/IdMoneda/Moneda se
    // resuelven vía el TARIFARIO de un pedido miembro (todos comparten el mismo IdTarifario).
    public class PedidoFacturaLineaListaConsulta
    {
        public int IdPedidoFacturaLinea { get; set; }
        public int? IdDocumentoElectronico { get; set; }
        public string? Codigo { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public int? IdTipoTramite { get; set; }
        public string? TipoTramite { get; set; }
        public int? IdMoneda { get; set; }
        public string? Moneda { get; set; }
        public int Cantidad { get; set; }
        public decimal ValorUnitario { get; set; }
        public decimal Descuento { get; set; }
    }

    public class PedidoFacturaLineaListaResult
    {
        public List<PedidoFacturaLineaListaConsulta> Lineas { get; set; } = new();
    }

    // Resultado de SP_PedidoFacturaLinea_ObtenerParaBorrador (PedidoFacturaLineaDAO.ObtenerParaBorradorAsync).
    public class PedidoFacturaLineaParaBorradorConsulta
    {
        public int IdPedidoFacturaLinea { get; set; }
        public int? IdDocumentoElectronico { get; set; }
        public string? Codigo { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal ValorUnitario { get; set; }
        public decimal Descuento { get; set; }
    }

    public class LineasParaBorradorConsulta
    {
        public List<PedidoFacturaLineaParaBorradorConsulta> Lineas { get; set; } = new();
    }
}