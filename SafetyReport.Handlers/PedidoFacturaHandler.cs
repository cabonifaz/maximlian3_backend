using Microsoft.Extensions.Logging;
using SafetyReport.DAO;
using SafetyReport.Models;

namespace SafetyReport.Handlers
{
    public class PedidoFacturaHandler
    {
        private readonly PedidoFacturaDAO _pedidoFacturaDao;
        private readonly PedidoDAO _pedidoDao;
        private readonly FacturacionElectronicaService _facturacionService;
        private readonly ILogger<PedidoFacturaHandler> _logger;

        public PedidoFacturaHandler(
            PedidoFacturaDAO pedidoFacturaDao, PedidoDAO pedidoDao, FacturacionElectronicaService facturacionService,
            ILogger<PedidoFacturaHandler> logger)
        {
            _pedidoFacturaDao = pedidoFacturaDao;
            _pedidoDao = pedidoDao;
            _facturacionService = facturacionService;
            _logger = logger;
        }

        public Task<Respuesta> ListarPedidosParaFacturacionAsync(UsuarioGeneral usuarioLogueado, ListarPedidosFacturacionRequest request) =>
            _pedidoDao.ListarParaFacturacionAsync(usuarioLogueado, request);

        // Dado un pedido, resuelve su IdDocumentoElectronico (PEDIDO_FACTURA) y trae la factura ya
        // guardada en ms-facturación, para que el front la use como base de edición (guardarCambios).
        public async Task<Respuesta> ObtenerFacturaPorPedidoAsync(UsuarioGeneral usuarioLogueado, int idPedido)
        {
            try
            {
                var idDocumento = await _pedidoFacturaDao.ObtenerIdDocumentoElectronicoAsync(usuarioLogueado, idPedido);
                if (idDocumento.IdTipoMensaje != 2 || idDocumento.Result is not PedidoFacturaIdDocumentoConsulta datos)
                {
                    return new Respuesta { IdTipoMensaje = idDocumento.IdTipoMensaje, Mensaje = idDocumento.Mensaje };
                }

                var documento = await _facturacionService.ObtenerDocumentoAsync(
                    usuarioLogueado.IdEmpresa, datos.IdDocumentoElectronico, CancellationToken.None);

                if (documento is null || documento.IdTipoMensaje != 2)
                {
                    return new Respuesta { IdTipoMensaje = documento?.IdTipoMensaje ?? 3, Mensaje = documento?.Mensaje ?? "No se pudo obtener el documento electrónico." };
                }

                return new Respuesta { IdTipoMensaje = 2, Mensaje = "Consulta exitosa.", Result = documento.Datos };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message };
            }
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

                if (pedidosPorId.Values.Any(p => p.Precio is null))
                {
                    return new Respuesta { IdTipoMensaje = 1, Mensaje = "Uno o más pedidos no tienen un tarifario con precio configurado." };
                }

                var facturacionRequest = new FacturacionInsertarDocumentoRequest
                {
                    IdInquilino = usuarioLogueado.IdEmpresa,
                    IdEmpresa = 1, // TODO: resolver desde EMPRESAS de ms-facturación (GET /api/v1/empresas?idInquilino=) en vez de fijo.
                    IdExterno = string.Join(",", request.lineas.Select(l => l.idPedido)),
                    NumeroReferencia = request.numeroReferencia,
                    IdTipoDocumentoMaestro = request.idTipoDocumentoMaestro,
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
                        ValorUnitario = pedidosPorId[l.idPedido].Precio.Value,
                        MontoDescuento = l.montoDescuento,
                        IdAfectacionIgvMaestro = l.idAfectacionIgvMaestro,
                        PorcentajeIgv = l.porcentajeIgv
                    }).ToList()
                };

                var insertado = await _facturacionService.InsertarDocumentoAsync(facturacionRequest, CancellationToken.None);
                if (insertado is null || insertado.IdTipoMensaje != 2 || insertado.Datos is null)
                {
                    return new Respuesta { IdTipoMensaje = insertado?.IdTipoMensaje ?? 3, Mensaje = insertado?.Mensaje ?? "No se pudo crear el documento electrónico en facturación." };
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

        // Edita un documento existente en PendienteEnvio (lineas/cuotas/formaPago/numeroReferencia). El
        // cliente no se toca acá — ms-facturación no lo permite (solo se fija una vez, al Insertar).
        public async Task<Respuesta> GuardarCambiosFacturaAsync(
            UsuarioGeneral usuarioLogueado, int idDocumentoElectronico, GuardarCambiosFacturaRequest request)
        {
            try
            {
                if (request.lineas.Count == 0)
                {
                    return new Respuesta { IdTipoMensaje = 1, Mensaje = "La factura debe tener al menos una línea." };
                }

                var idPedidos = request.lineas.Select(l => l.idPedido).Distinct().ToList();

                var datosBorrador = await _pedidoFacturaDao.ObtenerDatosBorradorAsync(usuarioLogueado, null, idPedidos);
                if (datosBorrador.IdTipoMensaje != 2 || datosBorrador.Result is not DatosBorradorFacturaConsulta datos)
                {
                    return new Respuesta { IdTipoMensaje = datosBorrador.IdTipoMensaje, Mensaje = datosBorrador.Mensaje };
                }

                var pedidosPorId = datos.Pedidos.ToDictionary(p => p.IdPedido);

                if (pedidosPorId.Values.Any(p => p.Precio is null))
                {
                    return new Respuesta { IdTipoMensaje = 1, Mensaje = "Uno o más pedidos no tienen un tarifario con precio configurado." };
                }

                var facturacionRequest = new FacturacionGuardarCambiosRequest
                {
                    IdFormaPago = request.idFormaPago,
                    NumeroReferencia = request.numeroReferencia,
                    IdMonedaMaestro = request.idMonedaMaestro,
                    IdTipoOperacionMaestro = request.idTipoOperacionMaestro,
                    Lineas = request.lineas.Select((l, i) => new FacturacionLineaEdicion
                    {
                        NumeroLinea = i + 1,
                        ProductoCodigo = pedidosPorId[l.idPedido].Codigo,
                        ProductoSunatCodigo = l.productoSunatCodigo,
                        Descripcion = pedidosPorId[l.idPedido].NombreCliente ?? string.Empty,
                        UnidadMedidaCodigo = l.unidadMedidaCodigo,
                        Cantidad = l.cantidad,
                        ValorUnitario = pedidosPorId[l.idPedido].Precio.Value,
                        MontoDescuento = l.montoDescuento,
                        IdAfectacionIgvMaestro = l.idAfectacionIgvMaestro,
                        PorcentajeIgv = l.porcentajeIgv,
                        IdLineaDocumentoElectronico = l.idLineaDocumentoElectronico
                    }).ToList(),
                    Cuotas = request.cuotas.Select(c => new FacturacionCuotaEdicion
                    {
                        NumeroCuota = c.numeroCuota,
                        FechaVencimiento = c.fechaVencimiento,
                        Monto = c.monto,
                        IdCuotaDocumentoElectronico = c.idCuotaDocumentoElectronico
                    }).ToList()
                };

                var resultado = await _facturacionService.GuardarCambiosAsync(
                    usuarioLogueado.IdEmpresa, idDocumentoElectronico, facturacionRequest, CancellationToken.None);

                if (resultado is null || resultado.IdTipoMensaje != 2)
                {
                    return new Respuesta { IdTipoMensaje = resultado?.IdTipoMensaje ?? 3, Mensaje = resultado?.Mensaje ?? "No se pudieron guardar los cambios en facturación." };
                }

                // Reconcilia PEDIDO_FACTURA con el nuevo set de pedidos: enlaza los que se agregaron,
                // desvincula los que se quitaron. Ninguna de las dos falla la operación si algo sale mal
                // acá — el documento en ms-facturación ya se guardó, solo queda desincronizado el vínculo.
                var enlace = await _pedidoFacturaDao.RegistrarEnvioAsync(
                    usuarioLogueado, idPedidos, idDocumentoElectronico, idEstadoFacturacion: 10);
                if (enlace.IdTipoMensaje != 2)
                {
                    _logger.LogWarning(
                        "No se pudo enlazar los pedidos {IdPedidos} al documento {IdDocumentoElectronico}: {Mensaje}",
                        string.Join(",", idPedidos), idDocumentoElectronico, enlace.Mensaje);
                }

                var desvinculacion = await _pedidoFacturaDao.DesvincularAsync(usuarioLogueado, idDocumentoElectronico, idPedidos);
                if (desvinculacion.IdTipoMensaje != 2)
                {
                    _logger.LogWarning(
                        "No se pudo desvincular los pedidos removidos del documento {IdDocumentoElectronico}: {Mensaje}",
                        idDocumentoElectronico, desvinculacion.Mensaje);
                }

                return new Respuesta { IdTipoMensaje = 2, Mensaje = "Cambios guardados correctamente." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message };
            }
        }

        public Task<Respuesta> ActualizarEstadoFacturacionAsync(UsuarioGeneral usuarioLogueado, int idPedido, int idEstadoFacturacion) =>
            _pedidoFacturaDao.ActualizarEstadoAsync(usuarioLogueado, idPedido, idEstadoFacturacion);

        // Confirma con SUNAT el documento ya guardado. ms-facturación recalcula FechaEmision/HoraEmision
        // a su propio reloj justo antes de enviar (ver EnviarDocumentoElectronicoASunatCasoDeUso) — no hace
        // falta que este Handler actualice nada antes de llamarlo.
        public async Task<Respuesta> EmitirFacturaAsync(UsuarioGeneral usuarioLogueado, int idDocumentoElectronico)
        {
            try
            {
                var resultado = await _facturacionService.EnviarASunatAsync(
                    usuarioLogueado.IdEmpresa, idDocumentoElectronico, CancellationToken.None);

                if (resultado is null || resultado.IdTipoMensaje != 2 || resultado.Datos is null)
                {
                    return new Respuesta { IdTipoMensaje = resultado?.IdTipoMensaje ?? 3, Mensaje = resultado?.Mensaje ?? "No se pudo emitir la factura." };
                }

                return new Respuesta { IdTipoMensaje = 2, Mensaje = "Factura emitida correctamente.", Result = resultado.Datos };
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

        public Task<Respuesta> ObtenerResumenDashboardAsync(UsuarioGeneral usuarioLogueado, DateOnly? fechaDesde, DateOnly? fechaHasta) =>
            _pedidoFacturaDao.ObtenerResumenAsync(usuarioLogueado, fechaDesde, fechaHasta);
    }
}
