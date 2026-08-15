namespace SafetyReport.Models
{
    public class FacturacionInsertarDocumentoRequest
    {
        public int IdInquilino { get; set; }
        public int IdEmpresa { get; set; }
        public string IdExterno { get; set; } = string.Empty;
        public string? NumeroReferencia { get; set; }
        public int IdTipoDocumentoMaestro { get; set; }
        public int IdMonedaMaestro { get; set; }
        public decimal? TipoCambio { get; set; }
        public int IdTipoOperacionMaestro { get; set; }
        // Null en Nota de Crédito/Débito: no tiene forma de pago propia (no existe cac:PaymentTerms en el
        // contenido documentado de CreditNote/DebitNote) — es un dato de la Factura/Boleta original.
        public FacturacionFormaPago? FormaPago { get; set; }
        public FacturacionCliente Cliente { get; set; } = new();
        public FacturacionDocumentoAfectado? DocumentoAfectado { get; set; }
        public List<FacturacionItem> Items { get; set; } = new();
        public List<FacturacionCampoExtraEntrada>? CamposExtra { get; set; }
    }

    public class FacturacionFormaPago
    {
        public int IdFormaPago { get; set; } = 1; // Num1 de TABLA_MAESTRA IdMaestro=9 (1=Contado, 2=Credito)
        public List<FacturacionCuota>? Cuotas { get; set; }
    }

    public class FacturacionCuota
    {
        public int NumeroCuota { get; set; }
        public DateOnly FechaVencimiento { get; set; }
        public decimal Monto { get; set; }
        // TABLA_MAESTRA IdMaestro=7 de ms-facturación (1=Pendiente, 2=Pagado) — el llamador lo decide
        // explícitamente, sin default implícito.
        public int IdEstadoCuotaMaestro { get; set; }
        // Debe ser coherente con IdEstadoCuotaMaestro: NULL si Pendiente, obligatoria si Pagado.
        public DateTime? FechaPago { get; set; }
    }

    public class FacturacionCliente
    {
        public int IdTipoDocumentoSunat { get; set; }
        public string NumeroDocumento { get; set; } = string.Empty;
        public string? Nombre { get; set; }
        public string? Correo { get; set; }
        public string? Direccion { get; set; }
        public int PaisCodigo { get; set; }
    }

    // Respuesta de GET .../para-nota — cliente + listado de productos de un documento ya emitido, para
    // prellenar/listar ambos al armar una Nota de Crédito/Débito contra ese documento.
    public class FacturacionDatosParaNota
    {
        public FacturacionCliente Cliente { get; set; } = new();
        // La Nota debe compartir la moneda y el tipo de cambio del documento afectado (obligatorio por
        // SUNAT) — ms-facturación la rechaza si no coinciden, así que el llamador necesita ambos valores
        // para prellenarla. TipoCambio es null cuando la moneda es PEN.
        public int IdMonedaMaestro { get; set; }
        public decimal? TipoCambio { get; set; }
        public List<FacturacionProductoResumen> Productos { get; set; } = new();
    }

    // Respuesta de GET .../resumen — dashboard de PedidoFactura. MontoTotalPEN es neto de Notas
    // (Factura/Boleta + Nota de Débito − Nota de Crédito), siempre en PEN; CantidadFacturas cuenta solo
    // Factura/Boleta. Ver SP_DocumentoElectronico_ObtenerResumenFacturacion.
    public class FacturacionResumenFacturacion
    {
        public int CantidadFacturas { get; set; }
        public decimal MontoTotalPEN { get; set; }
        public decimal? PromedioIngresoPEN { get; set; }
        public string MonedaIcono { get; set; } = string.Empty;
    }

    // Solo el código de producto de cada línea del documento — referencia para el usuario, no se copia
    // cantidad/precio/IGV.
    public class FacturacionProductoResumen
    {
        public int NumeroLinea { get; set; }
        public string ProductoCodigo { get; set; } = string.Empty;
    }

    public class FacturacionDocumentoAfectado
    {
        public int IdDocumentoElectronicoRelacionado { get; set; }
        public int IdMotivoMaestro { get; set; }
    }

    public class FacturacionItem
    {
        public int NumeroLinea { get; set; }
        public string ProductoCodigo { get; set; } = string.Empty;
        public string? ProductoSunatCodigo { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public int IdUnidadMedidaMaestro { get; set; }
        public decimal Cantidad { get; set; }
        public decimal ValorUnitario { get; set; }
        public decimal MontoDescuento { get; set; }
        public int IdAfectacionIgvMaestro { get; set; }
        public decimal PorcentajeIgv { get; set; }
    }

    // Payload de PUT api/v1/documentos-electronicos/{id}/guardar-cambios.
    public class FacturacionGuardarCambiosRequest
    {
        // Null en Nota de Crédito/Débito, mismo criterio que FacturacionInsertarDocumentoRequest.FormaPago.
        public int? IdFormaPago { get; set; }
        public string? NumeroReferencia { get; set; }
        public int IdMonedaMaestro { get; set; }
        public decimal? TipoCambio { get; set; }
        public int IdTipoOperacionMaestro { get; set; }
        // Solo aplica a Nota de Crédito/Débito (null en Factura/Boleta) — a diferencia de documentoAfectado
        // (fijo desde Insertar), el motivo sí es editable mientras el documento siga PendienteEnvio.
        public int? IdMotivoMaestro { get; set; }
        public List<FacturacionLineaEdicion> Lineas { get; set; } = new();
        public List<FacturacionCuotaEdicion> Cuotas { get; set; } = new();
        public List<FacturacionCampoExtraEdicion>? CamposExtra { get; set; }
    }

    // Campo extra dentro de guardar-cambios — IdCampoExtraDocumentoElectronico 0 (u omitido) = nuevo,
    // >0 = actualizar uno existente. Distinto de FacturacionCampoExtraEntrada (usado en Insertar/InsertarLote,
    // que no tiene Id porque ahí nada existe todavía).
    public class FacturacionCampoExtraEdicion
    {
        public string Texto { get; set; } = string.Empty;
        public int IdCampoExtraDocumentoElectronico { get; set; }
    }

    public class FacturacionLineaEdicion
    {
        public string ProductoCodigo { get; set; } = string.Empty;
        public string? ProductoSunatCodigo { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public int IdUnidadMedidaMaestro { get; set; }
        public decimal Cantidad { get; set; }
        public decimal ValorUnitario { get; set; }
        public decimal MontoDescuento { get; set; }
        public int IdAfectacionIgvMaestro { get; set; }
        public decimal PorcentajeIgv { get; set; }
        public int NumeroLinea { get; set; }
        public int IdLineaDocumentoElectronico { get; set; }
    }

    public class FacturacionCuotaEdicion
    {
        public int NumeroCuota { get; set; }
        public DateOnly FechaVencimiento { get; set; }
        public decimal Monto { get; set; }
        public int IdCuotaDocumentoElectronico { get; set; }
        // Ver FacturacionCuota.IdEstadoCuotaMaestro/FechaPago.
        public int IdEstadoCuotaMaestro { get; set; }
        public DateTime? FechaPago { get; set; }
    }

    public class FacturacionEnvelope<T>
    {
        public int IdTipoMensaje { get; set; }
        public string? Mensaje { get; set; }
        public T? Datos { get; set; }
    }

    public class FacturacionDocumentoCreado
    {
        public int IdDocumentoElectronico { get; set; }
        public string Serie { get; set; } = string.Empty;
        public int Correlativo { get; set; }
        public string EstadoCodigo { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
    }

    public class FacturacionResultadoEnvioSunat
    {
        public int EstadoCodigo { get; set; } // EstadoMaestroCodigo: 3=Aceptado, 4=AceptadoConObservaciones, 5=Rechazado, 8=Error
        public string? SunatCodigoRespuesta { get; set; }
        public string? SunatDescripcionRespuesta { get; set; }
    }

    // Body de PUT .../anular-manualmente — para cuando SUNAT ya muestra el documento como anulado sin que
    // este sistema haya tramitado esa baja. FechaAnulacion es la fecha real en que ocurrió, no "ahora".
    public class FacturacionAnularManualmenteRequest
    {
        public string Motivo { get; set; } = string.Empty;
        public DateTime FechaAnulacion { get; set; }
    }

    // Respuesta de PUT .../anular-manualmente y .../estado-sunat.
    public class FacturacionEstadoDocumentoActualizado
    {
        public int IdDocumentoElectronico { get; set; }
        public string EstadoCodigo { get; set; } = string.Empty;
    }

    // Fila de GET .../documentos-electronicos/eventos-recientes — usada por el worker de sincronización
    // para detectar el resultado de una Comunicación de Baja (async, sendSummary/getStatus).
    public class FacturacionEventoDocumento
    {
        public int IdEventoDocumento { get; set; }
        public int IdDocumentoElectronico { get; set; }
        public int IdEstadoNuevoMaestro { get; set; }
        public string EstadoCodigo { get; set; } = string.Empty;
        public bool EsAnulacion { get; set; }
    }

    // Fila de GET .../documentos-electronicos/{id}/errores-ultimo-envio — solo los errores/observaciones
    // del último intento de envío a SUNAT, no el historial completo de reintentos anteriores.
    public class FacturacionErrorDocumento
    {
        public int IdErrorDocumento { get; set; }
        public string OrigenErrorCodigo { get; set; } = string.Empty;
        public string CodigoError { get; set; } = string.Empty;
        public string MensajeError { get; set; } = string.Empty;
        public string? Campo { get; set; }
        public string SeveridadCodigo { get; set; } = string.Empty;
        public DateTime FchCre { get; set; }
    }

    // Fila de GET .../campos-extra — texto libre que el usuario agrega a un documento, sin relación con el
    // esquema SUNAT.
    public class FacturacionCampoExtra
    {
        public int IdCampoExtraDocumentoElectronico { get; set; }
        public string Texto { get; set; } = string.Empty;
    }

    // Body de PUT .../cuotas/{id}/estado — TABLA_MAESTRA IdMaestro=7 de ms-facturación (1=Pendiente, 2=Pagado).
    public class FacturacionActualizarEstadoCuotaRequest
    {
        public int EstadoCuotaCodigo { get; set; }
        // Debe ser coherente con EstadoCuotaCodigo: NULL si Pendiente, obligatoria si Pagado.
        public DateTime? FechaPago { get; set; }
    }

    // Respuesta de PUT .../cuotas/{id}/estado — la cuota ya actualizada.
    public class FacturacionCuotaActualizada
    {
        public int IdCuotaDocumentoElectronico { get; set; }
        public int NumeroCuota { get; set; }
        public DateOnly FechaVencimiento { get; set; }
        public decimal Monto { get; set; }
        public string EstadoCuotaCodigo { get; set; } = string.Empty;
        public DateTime? FechaPago { get; set; }
    }

    public class FacturacionInsertarCampoExtraRequest
    {
        public int IdInquilino { get; set; }
        public int IdDocumentoElectronico { get; set; }
        public string Texto { get; set; } = string.Empty;
    }

    public class FacturacionInsertarLoteCamposExtraRequest
    {
        public int IdInquilino { get; set; }
        public int IdDocumentoElectronico { get; set; }
        public List<FacturacionCampoExtraEntrada> CamposExtra { get; set; } = new();
    }

    public class FacturacionCampoExtraEntrada
    {
        public string Texto { get; set; } = string.Empty;
    }

    // Payload de POST api/v1/lotes-documento/comunicacion-baja. Todos los documentos deben compartir la
    // misma FechaReferencia (regla SUNAT); ms-facturación valida eso contra la FechaEmision real de cada uno.
    public class FacturacionComunicacionBajaRequest
    {
        public int IdInquilino { get; set; }
        public int IdEmpresa { get; set; }
        public DateOnly FechaReferencia { get; set; }
        public List<FacturacionItemBaja> Items { get; set; } = new();
    }

    public class FacturacionItemBaja
    {
        public int IdDocumentoElectronico { get; set; }
        public string MotivoDescripcion { get; set; } = string.Empty;
    }

    // sendSummary nunca resuelve en la misma llamada: el resultado esperable de éxito es un ticket, no un
    // veredicto — el veredicto real llega después vía SincronizacionFacturacionWorker.
    public class FacturacionLoteDocumentoCreado
    {
        public int IdLoteDocumento { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string EstadoCodigo { get; set; } = string.Empty;
        public DateTime FechaGeneracion { get; set; }
    }

    // Respuesta de GET api/v1/documentos-electronicos/para-pedido-factura (ms-facturación).
    public class FacturacionResultadoPaginado<T>
    {
        public int TotalRegistros { get; set; }
        public int TotalPaginas { get; set; }
        public List<T> Items { get; set; } = new();
    }

    public class FacturacionFacturaResumen
    {
        public int IdDocumentoElectronico { get; set; }
        public string NumeroFactura { get; set; } = string.Empty;
        // "Factura"/"Boleta de venta"/"Nota de crédito"/"Nota de débito".
        public string TipoDocumentoTexto { get; set; } = string.Empty;
        // Serie-Correlativo del documento afectado (p.ej. "F003-1556"); solo para Nota de Crédito/Débito, NULL en Factura/Boleta.
        public string? DocumentoAfectado { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
        public DateOnly FechaEmision { get; set; }
        public string FormaPagoCodigo { get; set; } = string.Empty;
        public decimal TotalImporte { get; set; }
        public string MonedaIcono { get; set; } = string.Empty;
        // Si el documento tiene una Comunicación de Baja en curso o ya aceptada, EstadoCodigo/ColorLetra/
        // ColorFondo ya reflejan ESE estado en vez del de emisión (ver SP_DocumentoElectronico_
        // ListarParaPedidoFactura) — un solo badge por fila, nunca dos superpuestos.
        public string EstadoCodigo { get; set; } = string.Empty;
        public string ColorLetra { get; set; } = string.Empty;
        public string ColorFondo { get; set; } = string.Empty;
    }
}
