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
        public int IdTipoOperacionMaestro { get; set; }
        public FacturacionFormaPago FormaPago { get; set; } = new();
        public FacturacionCliente Cliente { get; set; } = new();
        public FacturacionDocumentoAfectado? DocumentoAfectado { get; set; }
        public List<FacturacionItem> Items { get; set; } = new();
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

    public class FacturacionDocumentoAfectado
    {
        public int IdDocumentoElectronicoRelacionado { get; set; }
        public string TipoReferenciaCodigo { get; set; } = string.Empty;
        public string MotivoCodigo { get; set; } = string.Empty;
        public string MotivoDescripcion { get; set; } = string.Empty;
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
        public int IdFormaPago { get; set; }
        public string? NumeroReferencia { get; set; }
        public int IdMonedaMaestro { get; set; }
        public int IdTipoOperacionMaestro { get; set; }
        public List<FacturacionLineaEdicion> Lineas { get; set; } = new();
        public List<FacturacionCuotaEdicion> Cuotas { get; set; } = new();
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
        public string ClienteNombre { get; set; } = string.Empty;
        public DateOnly FechaEmision { get; set; }
        public string FormaPagoCodigo { get; set; } = string.Empty;
        public decimal TotalImporte { get; set; }
        public string MonedaIcono { get; set; } = string.Empty;
        public string EstadoCodigo { get; set; } = string.Empty;
        public string ColorLetra { get; set; } = string.Empty;
        public string ColorFondo { get; set; } = string.Empty;
    }
}
