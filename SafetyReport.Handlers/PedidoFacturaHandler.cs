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
        // Líneas/cuotas/tipo de documento vienen del front (son propios de esta factura, no de un maestro).
        // El cliente NO viene del front: solo manda idCliente, este Handler resuelve los datos vigentes en
        // CLIENTE (via SP_Cliente_ObtenerParaFacturacion) para no duplicar/desincronizar lo que ya está
        // guardado ahí. Este backend además agrega IdInquilino (=IdEmpresa del usuario).
        public async Task<Respuesta> GuardarBorradorFacturaAsync(UsuarioGeneral usuarioLogueado, GuardarBorradorFacturaRequest request)
        {
            try
            {
                if (request.lineas.Count == 0)
                {
                    return new Respuesta { IdTipoMensaje = 1, Mensaje = "La factura debe tener al menos una línea." };
                }

                var idPedidos = request.lineas.Select(l => l.idPedido).Distinct().ToList();

                var datosBorrador = await _pedidoFacturaDao.ObtenerDatosBorradorAsync(usuarioLogueado, request.idCliente, idPedidos);
                if (datosBorrador.IdTipoMensaje != 2 || datosBorrador.Result is not DatosBorradorFacturaConsulta datos)
                {
                    return new Respuesta { IdTipoMensaje = datosBorrador.IdTipoMensaje, Mensaje = datosBorrador.Mensaje };
                }

                var clienteDatos = datos.Cliente;
                var pedidosPorId = datos.Pedidos.ToDictionary(p => p.IdPedido);

                var facturacionRequest = new FacturacionInsertarDocumentoRequest
                {
                    IdInquilino = usuarioLogueado.IdEmpresa,
                    IdEmpresa = 1, // TODO: resolver desde EMPRESAS de ms-facturación (GET /api/v1/empresas?idInquilino=) en vez de fijo.
                    IdExterno = string.Join(",", request.lineas.Select(l => l.idPedido)),
                    IdTipoDocumentoMaestro = request.idTipoDocumentoMaestro,
                    FechaEmision = request.fechaEmision,
                    HoraEmision = request.horaEmision,
                    IdMonedaMaestro = request.idMonedaMaestro,
                    IdTipoOperacionMaestro = request.idTipoOperacionMaestro,
                    FormaPago = new FacturacionFormaPago
                    {
                        IdFormaPago = request.idFormaPago,
                        Cuotas = request.cuotas?.Select(c => new FacturacionCuota
                        {
                            NumeroCuota = c.numeroCuota,
                            FechaVencimiento = c.fechaVencimiento,
                            Monto = c.monto
                        }).ToList()
                    },
                    Cliente = new FacturacionCliente
                    {
                        IdTipoDocumentoSunat = clienteDatos.IdTipoDocumentoSunat,
                        NumeroDocumento = clienteDatos.NumeroDocumento,
                        Nombre = clienteDatos.Nombre,
                        Correo = clienteDatos.Correo,
                        Direccion = clienteDatos.Direccion,
                        PaisCodigo = clienteDatos.IdPais
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
                        ProductoCodigo = pedidosPorId[l.idPedido].Codigo,
                        ProductoSunatCodigo = l.productoSunatCodigo,
                        Descripcion = pedidosPorId[l.idPedido].NombreCliente ?? string.Empty,
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
                // en PEDIDO_FACTURA para todos los pedidos referenciados por las líneas en un solo UPDATE.
                var registro = await _pedidoFacturaDao.RegistrarEnvioAsync(
                    usuarioLogueado, idPedidos, insertado.Datos.IdDocumentoElectronico, idEstadoFacturacion: 10);

                if (registro.IdTipoMensaje != 2)
                {
                    _logger.LogWarning(
                        "No se pudo registrar el borrador de facturación para los pedidos {IdPedidos}: {Mensaje}",
                        string.Join(",", idPedidos), registro.Mensaje);
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
