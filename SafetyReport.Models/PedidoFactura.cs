namespace SafetyReport.Models
{
    public class GuardarBorradorFacturaRequest
    {
        public int idTipoDocumentoMaestro { get; set; }
        public string? numeroReferencia { get; set; }
        public int idMonedaMaestro { get; set; }
        public int idTipoOperacionMaestro { get; set; }
        public int idFormaPago { get; set; }
        public List<GuardarBorradorFacturaCuota>? cuotas { get; set; }
        public int idCliente { get; set; }
        public GuardarBorradorFacturaDocumentoAfectado? documentoAfectado { get; set; }
        public List<GuardarBorradorFacturaLinea> lineas { get; set; } = new();
    }

    public class GuardarBorradorFacturaCuota
    {
        public int numeroCuota { get; set; }
        public DateOnly fechaVencimiento { get; set; }
        public decimal monto { get; set; }
    }

    public class GuardarBorradorFacturaDocumentoAfectado
    {
        public int idDocumentoElectronicoRelacionado { get; set; }
        public string tipoReferenciaCodigo { get; set; } = string.Empty;
        public string motivoCodigo { get; set; } = string.Empty;
        public string motivoDescripcion { get; set; } = string.Empty;
    }

    // productoCodigo/descripcion/valorUnitario no vienen del front: se resuelven desde el propio Pedido
    // (Codigo/NombreCliente/TARIFARIO.Precio), mismo criterio que idCliente.
    public class GuardarBorradorFacturaLinea
    {
        public int idPedido { get; set; }
        public string? productoSunatCodigo { get; set; }
        public string unidadMedidaCodigo { get; set; } = string.Empty;
        public decimal cantidad { get; set; }
        public decimal montoDescuento { get; set; }
        public int idAfectacionIgvMaestro { get; set; }
        public decimal porcentajeIgv { get; set; }
    }

    // Editar un documento existente (PendienteEnvio) — mismo criterio que GuardarBorradorFacturaRequest:
    // el cliente no se edita acá (ms-facturación no lo permite); idTipoDocumentoMaestro tampoco (ya
    // consumió serie/correlativo en el Insertar). Todo lo demás del payload de guardarBorrador sí.
    public class GuardarCambiosFacturaRequest
    {
        public int idFormaPago { get; set; }
        public string? numeroReferencia { get; set; }
        public int idMonedaMaestro { get; set; }
        public int idTipoOperacionMaestro { get; set; }
        public List<GuardarCambiosFacturaLinea> lineas { get; set; } = new();
        public List<GuardarCambiosFacturaCuota> cuotas { get; set; } = new();
    }

    // productoCodigo/descripcion no vienen del front, mismo criterio que GuardarBorradorFacturaLinea.
    // idLineaDocumentoElectronico: 0 (u omitido) = línea nueva, >0 = actualizar una ya guardada.
    public class GuardarCambiosFacturaLinea
    {
        public int idPedido { get; set; }
        public string? productoSunatCodigo { get; set; }
        public string unidadMedidaCodigo { get; set; } = string.Empty;
        public decimal cantidad { get; set; }
        public decimal montoDescuento { get; set; }
        public int idAfectacionIgvMaestro { get; set; }
        public decimal porcentajeIgv { get; set; }
        public int numeroLinea { get; set; }
        public int idLineaDocumentoElectronico { get; set; }
    }

    public class GuardarCambiosFacturaCuota
    {
        public int numeroCuota { get; set; }
        public DateOnly fechaVencimiento { get; set; }
        public decimal monto { get; set; }
        public int idCuotaDocumentoElectronico { get; set; }
    }

    // Resultado de SP_PedidoFactura_ObtenerIdDocumentoElectronico.
    public class PedidoFacturaIdDocumentoConsulta
    {
        public int IdPedido { get; set; }
        public int IdDocumentoElectronico { get; set; }
        public int IdEstadoFacturacion { get; set; }
    }

    // Resultado de SP_Facturacion_ObtenerDatosBorrador — exactamente los campos que necesita el payload de facturación.
    public class PedidoParaFacturacionConsulta
    {
        public int IdPedido { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string? NombreCliente { get; set; }
        public string? NumReferencia { get; set; }
        public decimal? Precio { get; set; }
    }

    // Resultado de SP_Facturacion_ObtenerDatosBorrador (PedidoFacturaDAO.ObtenerDatosBorradorAsync).
    public class DatosBorradorFacturaConsulta
    {
        public ClienteParaFacturacionConsulta Cliente { get; set; } = new();
        public List<PedidoParaFacturacionConsulta> Pedidos { get; set; } = new();
    }

    // Query params de SP_Pedido_ListarParaFacturacion.
    public class ListarPedidosFacturacionRequest
    {
        public int idCliente { get; set; }
        public int? idTipoTramite { get; set; }
        public DateOnly? fechaInicio { get; set; }
        public DateOnly? fechaFin { get; set; }
        public int numPag { get; set; } = 1;
    }

    // Resultado de SP_Pedido_ListarParaFacturacion.
    public class PedidoListaFacturacionConsulta
    {
        public int IdPedido { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string? Investigado { get; set; }
        public string? AplicaPenalidad { get; set; }
        public string? TipoTramite { get; set; }
        public DateTime Fecha { get; set; }
        public decimal? Penalidad { get; set; }
        public decimal? Precio { get; set; }
        public decimal? DescuentoPorcentaje { get; set; }
    }

    public class PedidoListaFacturacionResult
    {
        public int TotalRegistros { get; set; }
        public int TotalPaginas { get; set; }
        public List<PedidoListaFacturacionConsulta> Pedidos { get; set; } = new();
    }

    // TABLA_MAESTRA IdMaestro=76 — checkpoint del worker de sincronización, un registro por empresa.
    public class CheckpointSincronizacionConsulta
    {
        public int IdEmpresa { get; set; }
        public int UltimoIdEvento { get; set; }
    }

    // Payload de POST PedidoFactura/anular. Todos los documentos deben compartir la misma FechaReferencia
    // (regla SUNAT) — el llamador la conoce porque elige facturas del mismo día para anular juntas.
    public class AnularFacturasRequest
    {
        public DateOnly FechaReferencia { get; set; }
        public List<AnularFacturaItem> Items { get; set; } = new();
    }

    public class AnularFacturaItem
    {
        public int IdDocumentoElectronico { get; set; }
        public string MotivoDescripcion { get; set; } = string.Empty;
    }
}