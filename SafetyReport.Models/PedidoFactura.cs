namespace SafetyReport.Models
{
    public class CampoExtraRequest
    {
        public string Texto { get; set; } = string.Empty;
    }

    // Body de PUT .../cuotas/{id}/estado — TABLA_MAESTRA IdMaestro=7 de ms-facturación (1=Pendiente, 2=Pagado).
    public class ActualizarEstadoCuotaRequest
    {
        public int idEstadoCuotaMaestro { get; set; }
        // Debe ser coherente con idEstadoCuotaMaestro: NULL si Pendiente, obligatoria si Pagado.
        public DateTime? fechaPago { get; set; }
    }

    // Body de PUT .../facturaPorId/{id}/anularManualmente — para cuando SUNAT ya muestra el documento como
    // anulado sin que este sistema haya tramitado esa baja. fechaAnulacion es la fecha real en que ocurrió.
    public class AnularManualmenteRequest
    {
        public string motivo { get; set; } = string.Empty;
        public DateTime fechaAnulacion { get; set; }
    }

    // SIRE RVIE: el TXT se genera al vuelo por request, nunca se guarda en S3 — mismo criterio de
    // exportación que CompaniaNoticiaDetalleExportacion.
    public class SireRvieExportacion
    {
        public string NombreArchivo { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public byte[] Archivo { get; set; } = [];
    }

    // Campo extra dentro de guardarBorrador/guardarCambios — idCampoExtraDocumentoElectronico es 0
    // (u omitido) para uno nuevo, o el id existente para actualizar uno ya guardado (solo aplica en
    // guardarCambios; guardarBorrador siempre inserta, así que ahí siempre viene en 0).
    public class CampoExtraEdicionRequest
    {
        public string Texto { get; set; } = string.Empty;
        public int idCampoExtraDocumentoElectronico { get; set; }
    }

    // Exclusivo para Factura/Boleta — la Nota de Crédito/Débito tiene su propio endpoint dedicado
    // (GenerarNotaCreditoDebitoRequest, notaCreditoDebito) porque no está atada a un Pedido; documentoAfectado
    // no viaja acá, no tendría de dónde salir (líneas se resuelven vía idPedido, nunca desde otro documento).
    public class GuardarBorradorFacturaRequest
    {
        public int idTipoDocumentoMaestro { get; set; }
        public string? numeroReferencia { get; set; }
        public int idMonedaMaestro { get; set; }
        public decimal? tipoCambio { get; set; }
        public int idTipoOperacionMaestro { get; set; }
        public int idFormaPago { get; set; }
        public List<GuardarBorradorFacturaCuota>? cuotas { get; set; }
        public int idCliente { get; set; }
        public List<GuardarBorradorFacturaLinea> lineas { get; set; } = new();
        public List<CampoExtraRequest>? camposExtra { get; set; }
    }

    public class GuardarBorradorFacturaCuota
    {
        public int numeroCuota { get; set; }
        public DateOnly fechaVencimiento { get; set; }
        public decimal monto { get; set; }
        // TABLA_MAESTRA IdMaestro=7 de ms-facturación (1=Pendiente, 2=Pagado) — sin default implícito.
        public int idEstadoCuotaMaestro { get; set; }
        // Debe ser coherente con idEstadoCuotaMaestro: NULL si Pendiente, obligatoria si Pagado.
        public DateTime? fechaPago { get; set; }
    }

    public class GuardarBorradorFacturaDocumentoAfectado
    {
        public int idDocumentoElectronicoRelacionado { get; set; }
        public int idMotivoMaestro { get; set; }
    }

    // Payload de guardarBorrador/notaCreditoDebito: a diferencia de GuardarBorradorFacturaRequest (pensado
    // para Factura/Boleta, donde cliente/línea se resuelven desde un Pedido vía idPedido/idCliente), acá el
    // front manda cliente e ítems completos porque una Nota de Crédito/Débito no está atada a un Pedido —
    // referencia otro documento electrónico ya emitido (documentoAfectado, obligatorio acá). Sin
    // idFormaPago/cuotas: una Nota de Crédito/Débito no tiene forma de pago propia (no existe
    // cac:PaymentTerms en el contenido documentado de CreditNote/DebitNote, Guía de Elaboración XML UBL 2.1
    // SUNAT) — es un dato de la Factura/Boleta original, la nota solo ajusta montos contra ella.
    public class GenerarNotaCreditoDebitoRequest
    {
        public int idTipoDocumentoMaestro { get; set; }
        public string? numeroReferencia { get; set; }
        public int idMonedaMaestro { get; set; }
        public decimal? tipoCambio { get; set; }
        public int idTipoOperacionMaestro { get; set; }
        public NotaCreditoDebitoCliente cliente { get; set; } = new();
        public GuardarBorradorFacturaDocumentoAfectado documentoAfectado { get; set; } = new();
        public List<NotaCreditoDebitoLinea> lineas { get; set; } = new();
        public List<CampoExtraRequest>? camposExtra { get; set; }
    }

    // Mismos campos que FacturacionCliente (ms-facturación) — acá el front lo manda completo, no se resuelve
    // desde CLIENTES vía idCliente como en GuardarBorradorFacturaRequest.
    public class NotaCreditoDebitoCliente
    {
        public int idTipoDocumentoSunat { get; set; }
        public string numeroDocumento { get; set; } = string.Empty;
        public string? nombre { get; set; }
        public string? correo { get; set; }
        public string? direccion { get; set; }
        public int paisCodigo { get; set; }
    }

    // productoCodigo/valorUnitario vienen completos del front acá (a diferencia de GuardarBorradorFacturaLinea,
    // que los resuelve desde Pedido/Tarifario) porque una línea de NC/ND no corresponde a un Pedido propio —
    // normalmente replica (total o parcialmente) una línea del documento afectado.
    public class NotaCreditoDebitoLinea
    {
        public string productoCodigo { get; set; } = string.Empty;
        public string? productoSunatCodigo { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public int idUnidadMedidaMaestro { get; set; }
        public decimal cantidad { get; set; }
        public decimal valorUnitario { get; set; }
        public decimal montoDescuento { get; set; }
        public int idAfectacionIgvMaestro { get; set; }
        public decimal porcentajeIgv { get; set; }
    }

    // Editar una Nota de Crédito/Débito existente (PendienteEnvio) — mismo criterio que
    // GuardarCambiosFacturaRequest, pero sin idPedido: las líneas siguen siendo texto libre completo, igual
    // que en GenerarNotaCreditoDebitoRequest. El cliente y idDocumentoElectronicoRelacionado no se editan
    // acá (se fijan al crear, ms-facturación no lo permite) — idMotivoMaestro sí, es un detalle de negocio
    // corregible mientras el documento siga PendienteEnvio. idDocumentoElectronicoRelacionado sí viaja en
    // el payload (aunque no se edite) porque IdExterno se recalcula igual que en
    // GenerarNotaCreditoDebitoRequest — el front ya lo conoce, se lo pidió al cargar la Nota para editarla.
    public class EditarNotaCreditoDebitoRequest
    {
        public string? numeroReferencia { get; set; }
        public int idDocumentoElectronicoRelacionado { get; set; }
        public int idMonedaMaestro { get; set; }
        public decimal? tipoCambio { get; set; }
        public int idTipoOperacionMaestro { get; set; }
        public int idMotivoMaestro { get; set; }
        public List<NotaCreditoDebitoLineaEdicion> lineas { get; set; } = new();
        public List<CampoExtraEdicionRequest>? camposExtra { get; set; }
    }

    // Mismo criterio que NotaCreditoDebitoLinea (todo texto libre, sin idPedido) + idLineaDocumentoElectronico
    // (0 u omitido = línea nueva, >0 = actualizar una ya guardada), igual que GuardarCambiosFacturaLinea.
    public class NotaCreditoDebitoLineaEdicion
    {
        public string productoCodigo { get; set; } = string.Empty;
        public string? productoSunatCodigo { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public int idUnidadMedidaMaestro { get; set; }
        public decimal cantidad { get; set; }
        public decimal valorUnitario { get; set; }
        public decimal montoDescuento { get; set; }
        public int idAfectacionIgvMaestro { get; set; }
        public decimal porcentajeIgv { get; set; }
        public int idLineaDocumentoElectronico { get; set; }
    }

    // ProductoCodigo/Descripcion/Cantidad/ValorUnitario/MontoDescuento no vienen del front: se
    // resuelven desde la propia PEDIDO_FACTURA_LINEA (ya congelados al crear/editar la línea, ver
    // SP_PedidoFacturaLinea_Crear/ActualizarPedidos) — este DTO solo aporta lo que la línea no
    // sabe: el mapeo a catálogos SUNAT del documento.
    public class GuardarBorradorFacturaLinea
    {
        public int idPedidoFacturaLinea { get; set; }
        public string? productoSunatCodigo { get; set; }
        public int idUnidadMedidaMaestro { get; set; }
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
        public decimal? tipoCambio { get; set; }
        public int idTipoOperacionMaestro { get; set; }
        public List<GuardarCambiosFacturaLinea> lineas { get; set; } = new();
        public List<GuardarCambiosFacturaCuota> cuotas { get; set; } = new();
        public List<CampoExtraEdicionRequest>? camposExtra { get; set; }
    }

    // ProductoCodigo/Descripcion/Cantidad/ValorUnitario/MontoDescuento vienen de la propia
    // PEDIDO_FACTURA_LINEA (congelados), mismo criterio que GuardarBorradorFacturaLinea — este
    // DTO solo aporta el mapeo a catálogos SUNAT. idLineaDocumentoElectronico: 0 (u omitido) =
    // línea nueva, >0 = actualizar una ya guardada.
    public class GuardarCambiosFacturaLinea
    {
        public int idPedidoFacturaLinea { get; set; }
        public string? productoSunatCodigo { get; set; }
        public int idUnidadMedidaMaestro { get; set; }
        public int idAfectacionIgvMaestro { get; set; }
        public decimal porcentajeIgv { get; set; }
        public int idLineaDocumentoElectronico { get; set; }
    }

    public class GuardarCambiosFacturaCuota
    {
        public int numeroCuota { get; set; }
        public DateOnly fechaVencimiento { get; set; }
        public decimal monto { get; set; }
        public int idCuotaDocumentoElectronico { get; set; }
        // Ver GuardarBorradorFacturaCuota.idEstadoCuotaMaestro/fechaPago.
        public int idEstadoCuotaMaestro { get; set; }
        public DateTime? fechaPago { get; set; }
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

    // Query params de SP_Pedido_ListarParaFacturacion. Sin paginación — devuelve todos los
    // pedidos elegibles de una — y el filtro de fecha es por mes (anio/mes), no por rango, ya
    // que agrupar pedidos de meses distintos en una línea no está permitido de todas formas
    // (ver PLAN_Lineas_Facturacion.md).
    public class ListarPedidosFacturacionRequest
    {
        public int idCliente { get; set; }
        public int idTipoTramite { get; set; }
        public int? anio { get; set; }
        public int? mes { get; set; }
        public List<int>? idsPais { get; set; }
        public int? idMoneda { get; set; }
    }

    // El request/DTO del CRUD de líneas vive en PedidoFacturaLinea.cs.

    // Filtros para el proxy hacia GET api/v1/documentos-electronicos/para-pedido-factura (ms-facturación).
    public class ListarFacturasRequest
    {
        public string? estadoCodigo { get; set; }
        public int? idFormaPago { get; set; } // Num1 de TABLA_MAESTRA IdMaestro=9 en ms-facturación (1=Contado, 2=Credito)
        public DateOnly? fechaDesde { get; set; }
        public DateOnly? fechaHasta { get; set; }
        public string? busqueda { get; set; }
        public int pagina { get; set; } = 1;
        public int tamanoPagina { get; set; } = 20;
    }

    // Resultado de SP_Pedido_ListarParaFacturacion. IdPais/IdTipoTramite/IdTarifario/IdMoneda
    // (además de sus etiquetas legibles) se exponen para que el front pueda anticipar la guarda
    // de agrupamiento de SP_PedidoFacturaLinea_Crear (mismo cliente/mes/IdTarifario) antes de
    // intentar crear la línea — la validación real sigue siendo del lado del SP.
    public class PedidoListaFacturacionConsulta
    {
        public int IdPedido { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string NumReferencia { get; set; } = string.Empty;
        public string? Investigado { get; set; }
        public int? IdPais { get; set; }
        public string? Pais { get; set; }
        public string? AplicaPenalidad { get; set; }
        public int? IdTipoTramite { get; set; }
        public string? TipoTramite { get; set; }
        public DateTime Fecha { get; set; }
        public int? IdTarifario { get; set; }
        public decimal? Penalidad { get; set; }
        public decimal? Precio { get; set; }
        public int? IdMoneda { get; set; }
        public string? Moneda { get; set; }
    }

    public class PedidoListaFacturacionResult
    {
        public List<PedidoListaFacturacionConsulta> Pedidos { get; set; } = new();
    }

    // Resultado de SP_Pedido_ListarPorDocumentoElectronico. Sin ids (no hacen falta) — Codigo es
    // el identificador visible del pedido. ValorUnitario/Descuento son los montos congelados de
    // PEDIDO_FACTURA_LINEA (no de TARIFARIO, ver PedidoFacturaLineaConsulta), ya concatenados con
    // el código de Moneda del lado del SP — se devuelven como string, no decimal.
    public class PedidoPorDocumentoElectronicoConsulta
    {
        public string Codigo { get; set; } = string.Empty;
        public string NumReferencia { get; set; } = string.Empty;
        public string? Investigado { get; set; }
        public string TipoTramite { get; set; } = string.Empty;
        public string Pais { get; set; } = string.Empty;
        public string ValorUnitario { get; set; } = string.Empty;
        public string Descuento { get; set; } = string.Empty;
    }

    public class PedidoPorDocumentoElectronicoResult
    {
        public List<PedidoPorDocumentoElectronicoConsulta> Pedidos { get; set; } = new();
    }

    // Payload de POST .../listarPedidos/exportarExcel para SP_Pedido_ListarParaPrefactura — el rango llega
    // por FchInicio/FchFin o por Meses (uno o varios pares Anio/Mes, no contiguos incluido), nunca los dos
    // a la vez ni ninguno de los dos (mismo chequeo que hace el SP). [FromBody]: Meses es una lista JSON,
    // no un query string — evita el formato frágil Meses[0].Anio=... que un GET hubiera necesitado.
    public class FiltroPedidoPrefactura
    {
        public int IdCliente { get; set; }
        public DateOnly? FchInicio { get; set; }
        public DateOnly? FchFin { get; set; }
        public List<AnioMesFiltro>? Meses { get; set; }
    }

    public class AnioMesFiltro
    {
        public int Anio { get; set; }
        public int Mes { get; set; }
    }

    // Resultado de SP_Pedido_ListarParaPrefactura — mismas 9 columnas en ambos idiomas (ver
    // 13_SP_Pedido_ListarParaPrefactura.sql), leídas por posición: el SP bifurca por
    // c.IdIdiomaFacturacion y devuelve encabezados en inglés o español, así que la DAO no puede
    // leer por nombre de columna (dr["COMPANY"] no existe en la rama en español). Los encabezados
    // reales para el Excel salen de PedidoPrefacturaResult.Headers, tal cual los devolvió el SP.
    public class PedidoPrefacturaConsulta
    {
        public string Company { get; set; } = string.Empty;
        public string TypeOfReport { get; set; } = string.Empty;
        public string ReferenceNumber { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string DateOfRequest { get; set; } = string.Empty;
        public string ApprovedOn { get; set; } = string.Empty;
        public string TypeOfService { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Observation { get; set; } = string.Empty;
    }

    public class PedidoPrefacturaResult
    {
        public string Moneda { get; set; } = string.Empty;
        public List<string> Headers { get; set; } = new();
        public List<PedidoPrefacturaConsulta> Items { get; set; } = new();
    }

    public class PedidoPrefacturaExportacion
    {
        public string NombreArchivo { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public byte[] Archivo { get; set; } = [];
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