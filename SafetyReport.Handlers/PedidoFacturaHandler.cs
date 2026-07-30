using Microsoft.Extensions.Logging;
using SafetyReport.DAO;
using SafetyReport.Models;

namespace SafetyReport.Handlers
{
    public class PedidoFacturaHandler
    {
        private readonly PedidoDAO _pedidoDao;
        private readonly ClienteDAO _clienteDao;
        private readonly PedidoFacturaDAO _pedidoFacturaDao;
        private readonly FacturacionElectronicaService _facturacionService;
        private readonly ILogger<PedidoFacturaHandler> _logger;

        // TODO: reemplazar por el IdEmpresa/IdSerieDocumento reales de ms-facturación una vez definido
        // cómo este backend relaciona su tenant con la Empresa/Serie correspondiente allá.
        private const int IdEmpresaFacturacionPlaceholder = 1;
        private const int IdSerieDocumentoPlaceholder = 1;

        public PedidoFacturaHandler(
            PedidoDAO pedidoDao, ClienteDAO clienteDao, PedidoFacturaDAO pedidoFacturaDao,
            FacturacionElectronicaService facturacionService, ILogger<PedidoFacturaHandler> logger)
        {
            _pedidoDao = pedidoDao;
            _clienteDao = clienteDao;
            _pedidoFacturaDao = pedidoFacturaDao;
            _facturacionService = facturacionService;
            _logger = logger;
        }

        public async Task<Respuesta> EnviarPedidoASunatAsync(UsuarioGeneral usuarioLogueado, int idPedido, string ambienteCodigo)
        {
            try
            {
                var respuestaPedido = await _pedidoDao.ObtenerAsync(usuarioLogueado, new FiltroPedidoObtener { idPedido = idPedido });
                var pedido = (respuestaPedido.Result as List<PedidoConsulta>)?.FirstOrDefault();
                if (respuestaPedido.IdTipoMensaje != 2 || pedido is null)
                {
                    return new Respuesta { IdTipoMensaje = 1, Mensaje = "No se encontró el pedido indicado." };
                }

                var respuestaCliente = await _clienteDao.ObtenerClienteAsync(usuarioLogueado, pedido.IdCliente);
                var cliente = (respuestaCliente.Result as List<ClienteConsulta>)?.FirstOrDefault();
                if (respuestaCliente.IdTipoMensaje != 2 || cliente is null)
                {
                    return new Respuesta { IdTipoMensaje = 1, Mensaje = "No se encontró el cliente del pedido." };
                }

                var ahora = DateTime.Now;

                // TODO: TipoDocumentoCodigo/TipoOperacionCodigo/FormaPago y el Item (producto/precio) son
                // placeholders — pendiente de definir la regla real de facturación por pedido (Tarifario).
                var request = new FacturacionInsertarDocumentoRequest
                {
                    IdInquilino = usuarioLogueado.IdEmpresa,
                    IdEmpresa = IdEmpresaFacturacionPlaceholder,
                    IdExterno = pedido.Codigo,
                    TipoDocumentoCodigo = "01",
                    IdSerieDocumento = IdSerieDocumentoPlaceholder,
                    FechaEmision = DateOnly.FromDateTime(ahora),
                    HoraEmision = TimeOnly.FromDateTime(ahora),
                    MonedaCodigo = "PEN",
                    TipoOperacionCodigo = "0101",
                    FormaPago = new FacturacionFormaPago { Codigo = "Contado" },
                    Cliente = new FacturacionCliente
                    {
                        TipoDocumentoCodigo = "6",
                        NumeroDocumento = cliente.NumRegistroTributario ?? string.Empty,
                        Nombre = cliente.Nombre,
                        Correo = cliente.Correo,
                        Direccion = cliente.Direccion
                    },
                    Items =
                    [
                        new FacturacionItem
                        {
                            NumeroLinea = 1,
                            ProductoCodigo = "SERVICIO",
                            Descripcion = pedido.Codigo,
                            UnidadMedidaCodigo = "ZZ",
                            Cantidad = 1,
                            ValorUnitario = 0,
                            PrecioUnitario = 0,
                            MontoDescuento = 0,
                            AfectacionIgvCodigo = "10",
                            PorcentajeIgv = 18
                        }
                    ]
                };

                var insertado = await _facturacionService.InsertarDocumentoAsync(request, CancellationToken.None);
                if (insertado?.Datos is null)
                {
                    return new Respuesta { IdTipoMensaje = 3, Mensaje = insertado?.Mensaje ?? "No se pudo crear el documento electrónico en facturación." };
                }

                var enviado = await _facturacionService.EnviarASunatAsync(
                    usuarioLogueado.IdEmpresa, insertado.Datos.IdDocumentoElectronico, ambienteCodigo, CancellationToken.None);
                if (enviado?.Datos is null)
                {
                    return new Respuesta { IdTipoMensaje = 3, Mensaje = enviado?.Mensaje ?? "No se pudo enviar el documento electrónico a SUNAT." };
                }

                // EstadoMaestroCodigo (ms-facturación): 3=Aceptado, 4=AceptadoConObservaciones -> Aprobado (5);
                // cualquier otro (5=Rechazado, 8=Error) -> Rechazado (6).
                var idEstadoFacturacion = enviado.Datos.EstadoCodigo is 3 or 4 ? 5 : 6;

                return await _pedidoFacturaDao.RegistrarEnvioAsync(
                    usuarioLogueado, idPedido, insertado.Datos.IdDocumentoElectronico, idEstadoFacturacion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message };
            }
        }
    }
}
