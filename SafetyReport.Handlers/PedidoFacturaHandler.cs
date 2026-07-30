using Microsoft.Extensions.Logging;
using SafetyReport.DAO;
using SafetyReport.Models;

namespace SafetyReport.Handlers
{
    public class PedidoFacturaHandler
    {
        private readonly PedidoFacturaDAO _pedidoFacturaDao;
        private readonly FacturacionElectronicaService _facturacionService;
        private readonly ILogger<PedidoFacturaHandler> _logger;

        public PedidoFacturaHandler(
            PedidoFacturaDAO pedidoFacturaDao, FacturacionElectronicaService facturacionService,
            ILogger<PedidoFacturaHandler> logger)
        {
            _pedidoFacturaDao = pedidoFacturaDao;
            _facturacionService = facturacionService;
            _logger = logger;
        }

        // Solo Guardar: crea el borrador en ms-facturación (PendienteEnvio), no lo envía a SUNAT.
        // Todos los datos de la factura (líneas/cuotas/cliente/tipo de documento) vienen del front —
        // este backend solo agrega IdInquilino (=IdEmpresa del usuario) y SistemaOrigen.
        public async Task<Respuesta> GuardarBorradorFacturaAsync(UsuarioGeneral usuarioLogueado, GuardarBorradorFacturaRequest request)
        {
            try
            {
                if (request.lineas.Count == 0)
                {
                    return new Respuesta { IdTipoMensaje = 1, Mensaje = "La factura debe tener al menos una línea." };
                }

                var facturacionRequest = new FacturacionInsertarDocumentoRequest
                {
                    IdInquilino = usuarioLogueado.IdEmpresa,
                    IdEmpresa = request.idEmpresa,
                    IdExterno = string.Join(",", request.lineas.Select(l => l.idPedido)),
                    TipoDocumentoCodigo = request.tipoDocumentoCodigo,
                    IdSerieDocumento = request.idSerieDocumento,
                    FechaEmision = request.fechaEmision,
                    HoraEmision = request.horaEmision,
                    MonedaCodigo = request.monedaCodigo,
                    TipoOperacionCodigo = request.tipoOperacionCodigo,
                    FormaPago = new FacturacionFormaPago
                    {
                        Codigo = request.formaPagoCodigo,
                        Cuotas = request.cuotas?.Select(c => new FacturacionCuota
                        {
                            NumeroCuota = c.numeroCuota,
                            FechaVencimiento = c.fechaVencimiento,
                            Monto = c.monto
                        }).ToList()
                    },
                    Cliente = new FacturacionCliente
                    {
                        TipoDocumentoCodigo = request.cliente.tipoDocumentoCodigo,
                        NumeroDocumento = request.cliente.numeroDocumento,
                        Nombre = request.cliente.nombre,
                        Correo = request.cliente.correo,
                        Direccion = request.cliente.direccion
                    },
                    DocumentoAfectado = request.documentoAfectado is null ? null : new FacturacionDocumentoAfectado
                    {
                        IdDocumentoElectronicoRelacionado = request.documentoAfectado.idDocumentoElectronicoRelacionado,
                        TipoReferenciaCodigo = request.documentoAfectado.tipoReferenciaCodigo,
                        MotivoCodigo = request.documentoAfectado.motivoCodigo,
                        MotivoDescripcion = request.documentoAfectado.motivoDescripcion
                    },
                    Items = request.lineas.Select((l, i) => new FacturacionItem
                    {
                        NumeroLinea = i + 1,
                        ProductoCodigo = l.productoCodigo,
                        ProductoSunatCodigo = l.productoSunatCodigo,
                        Descripcion = l.descripcion,
                        UnidadMedidaCodigo = l.unidadMedidaCodigo,
                        Cantidad = l.cantidad,
                        ValorUnitario = l.valorUnitario,
                        PrecioUnitario = l.precioUnitario,
                        MontoDescuento = l.montoDescuento,
                        AfectacionIgvCodigo = l.afectacionIgvCodigo,
                        PorcentajeIgv = l.porcentajeIgv
                    }).ToList()
                };

                var insertado = await _facturacionService.InsertarDocumentoAsync(facturacionRequest, CancellationToken.None);
                if (insertado?.Datos is null)
                {
                    return new Respuesta { IdTipoMensaje = 3, Mensaje = insertado?.Mensaje ?? "No se pudo crear el documento electrónico en facturación." };
                }

                // Un borrador puede cubrir varios pedidos: se registra el mismo IdDocumentoElectronico
                // en PEDIDO_FACTURA para cada pedido referenciado por una línea.
                var idPedidos = request.lineas.Select(l => l.idPedido).Distinct();
                foreach (var idPedido in idPedidos)
                {
                    var resultado = await _pedidoFacturaDao.RegistrarEnvioAsync(
                        usuarioLogueado, idPedido, insertado.Datos.IdDocumentoElectronico, idEstadoFacturacion: 10);

                    if (resultado.IdTipoMensaje != 2)
                    {
                        _logger.LogWarning(
                            "No se pudo registrar el borrador de facturación para el pedido {IdPedido}: {Mensaje}",
                            idPedido, resultado.Mensaje);
                    }
                }

                return ResultadoOperacionExito(insertado.Datos.IdDocumentoElectronico);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message };
            }
        }

        private static Respuesta ResultadoOperacionExito(int idDocumentoElectronico) => new()
        {
            IdTipoMensaje = 2,
            Mensaje = "Borrador de factura guardado correctamente.",
            Result = new { IdDocumentoElectronico = idDocumentoElectronico }
        };
    }
}
