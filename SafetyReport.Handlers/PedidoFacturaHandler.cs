using Microsoft.Extensions.Configuration;
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
        private readonly IConfiguration _configuration;
        private readonly ILogger<PedidoFacturaHandler> _logger;

        public PedidoFacturaHandler(
            PedidoFacturaDAO pedidoFacturaDao, PedidoDAO pedidoDao, FacturacionElectronicaService facturacionService,
            IConfiguration configuration, ILogger<PedidoFacturaHandler> logger)
        {
            _pedidoFacturaDao = pedidoFacturaDao;
            _pedidoDao = pedidoDao;
            _facturacionService = facturacionService;
            _configuration = configuration;
            _logger = logger;
        }

        public Task<Respuesta> ListarPedidosParaFacturacionAsync(UsuarioGeneral usuarioLogueado, ListarPedidosFacturacionRequest request) =>
            _pedidoDao.ListarParaFacturacionAsync(usuarioLogueado, request);

        // Listado de facturas ya generadas — NumeroFactura/ClienteNombre/FormaPago/Estado vienen resueltos
        // por ms-facturación, este Handler solo hace de proxy con los filtros del front.
        public async Task<Respuesta> ListarFacturasAsync(UsuarioGeneral usuarioLogueado, ListarFacturasRequest request)
        {
            try
            {
                var resultado = await _facturacionService.ListarFacturasAsync(
                    usuarioLogueado.IdEmpresa, // IdInquilino en ms-facturación = IdEmpresa acá
                    1, // TODO: resolver desde EMPRESAS de ms-facturación (GET /api/v1/empresas?idInquilino=) en vez de fijo.
                    request.estadoCodigo, request.idFormaPago, request.fechaDesde, request.fechaHasta, request.busqueda,
                    request.pagina, request.tamanoPagina, CancellationToken.None);

                if (resultado is null || resultado.IdTipoMensaje != 2)
                {
                    return new Respuesta { IdTipoMensaje = resultado?.IdTipoMensaje ?? 3, Mensaje = resultado?.Mensaje ?? "No se pudo obtener el listado de facturas." };
                }

                return new Respuesta { IdTipoMensaje = 2, Mensaje = "Consulta exitosa.", Result = resultado.Datos };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message };
            }
        }

        // Trae la factura directo por su propio id — cierra el hueco que dejaba ObtenerFacturaPorPedidoAsync
        // (que exige idPedido) para el listado nuevo (ListarFacturasAsync), que solo devuelve
        // IdDocumentoElectronico y no un idPedido único (una factura puede cubrir más de un pedido).
        public async Task<Respuesta> ObtenerFacturaPorIdAsync(UsuarioGeneral usuarioLogueado, int idDocumentoElectronico)
        {
            try
            {
                var documento = await _facturacionService.ObtenerDocumentoAsync(
                    usuarioLogueado.IdEmpresa, idDocumentoElectronico, CancellationToken.None);

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

        // Cliente + listado de productos de una Factura/Boleta ya emitida — para prellenar "cliente" y
        // listar los productos del documento afectado en GenerarNotaCreditoDebitoAsync, sin que el front
        // tenga que volver a tipear sus datos.
        public async Task<Respuesta> ObtenerParaNotaAsync(UsuarioGeneral usuarioLogueado, int idDocumentoElectronico)
        {
            try
            {
                var resultado = await _facturacionService.ObtenerParaNotaAsync(
                    usuarioLogueado.IdEmpresa, idDocumentoElectronico, CancellationToken.None);

                if (resultado is null || resultado.IdTipoMensaje != 2 || resultado.Datos is null)
                {
                    return new Respuesta { IdTipoMensaje = resultado?.IdTipoMensaje ?? 3, Mensaje = resultado?.Mensaje ?? "No se pudo obtener los datos del documento electrónico." };
                }

                return new Respuesta { IdTipoMensaje = 2, Mensaje = "Consulta exitosa.", Result = resultado.Datos };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message };
            }
        }

        // Arma el link público de verificación ({Cors:AllowedOrigins[0]}/factura/{token}) — el front de este
        // proyecto solo corre en un origin, así que reusar el primer AllowedOrigins evita mantener la misma
        // URL duplicada en dos claves de config. Si algún día AllowedOrigins tiene más de un origin real
        // (más de un front), esto deja de ser válido y hay que volver a una clave propia.
        // A diferencia de ObtenerUrlDescargaAsync, acá el token nunca sale de este backend hacia afuera;
        // solo se usa para componer la URL final que sí se comparte. Cualquiera con el link puede abrirlo
        // sin login (VerificacionFacturaController, sin [Authorize]).
        public async Task<Respuesta> ObtenerUrlVerificacionAsync(UsuarioGeneral usuarioLogueado, int idDocumentoElectronico)
        {
            try
            {
                var resultado = await _facturacionService.ObtenerTokenVerificacionAsync(
                    usuarioLogueado.IdEmpresa, idDocumentoElectronico, CancellationToken.None);

                if (resultado is null || resultado.IdTipoMensaje != 2 || string.IsNullOrEmpty(resultado.Datos))
                {
                    return new Respuesta { IdTipoMensaje = resultado?.IdTipoMensaje ?? 3, Mensaje = resultado?.Mensaje ?? "No se pudo obtener el link de verificación." };
                }

                var frontendUrl = _configuration.GetSection("Cors:AllowedOrigins").GetChildren().FirstOrDefault()?.Value
                    ?? throw new InvalidOperationException("No se configuró Cors:AllowedOrigins.");

                var url = $"{frontendUrl.TrimEnd('/')}/factura/{resultado.Datos}";
                return new Respuesta { IdTipoMensaje = 2, Mensaje = "Consulta exitosa.", Result = url };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message };
            }
        }

        // tipoArchivo: "Xml" o "Pdf". Solo hace de proxy — la URL presignada la arma ms-facturación.
        public async Task<Respuesta> ObtenerUrlDescargaAsync(UsuarioGeneral usuarioLogueado, int idDocumentoElectronico, string tipoArchivo)
        {
            try
            {
                var resultado = await _facturacionService.ObtenerUrlDescargaAsync(
                    usuarioLogueado.IdEmpresa, idDocumentoElectronico, tipoArchivo, CancellationToken.None);

                if (resultado is null || resultado.IdTipoMensaje != 2)
                {
                    return new Respuesta { IdTipoMensaje = resultado?.IdTipoMensaje ?? 3, Mensaje = resultado?.Mensaje ?? "No se pudo obtener la URL de descarga." };
                }

                return new Respuesta { IdTipoMensaje = 2, Mensaje = "Consulta exitosa.", Result = resultado.Datos };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message };
            }
        }

        // Solo los errores/observaciones del último intento de envío a SUNAT (no el historial completo de
        // reintentos anteriores) — ver SP_ErrorDocumento_ListarUltimoEnvio en ms-facturación.
        public async Task<Respuesta> ObtenerErroresUltimoEnvioAsync(UsuarioGeneral usuarioLogueado, int idDocumentoElectronico)
        {
            try
            {
                var resultado = await _facturacionService.ObtenerErroresUltimoEnvioAsync(
                    usuarioLogueado.IdEmpresa, idDocumentoElectronico, CancellationToken.None);

                if (resultado is null || resultado.IdTipoMensaje != 2)
                {
                    return new Respuesta { IdTipoMensaje = resultado?.IdTipoMensaje ?? 3, Mensaje = resultado?.Mensaje ?? "No se pudieron obtener los errores del último envío." };
                }

                return new Respuesta { IdTipoMensaje = 2, Mensaje = "Consulta exitosa.", Result = resultado.Datos };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message };
            }
        }

        // TXT SIRE RVIE — generado al vuelo por ms-facturación, nunca se guarda en S3. Se propaga tal cual
        // (Result = SireRvieExportacion) para que el controller lo devuelva como File(...) en vez de JSON.
        public async Task<Respuesta> GenerarTxtSireRvieAsync(UsuarioGeneral usuarioLogueado, DateOnly periodo)
        {
            try
            {
                var idEmpresa = 1; // TODO: resolver desde EMPRESAS de ms-facturación, mismo TODO que GuardarBorradorFacturaAsync.
                var (exito, mensaje, contenido, nombreArchivo) = await _facturacionService.ObtenerTxtSireRvieAsync(
                    usuarioLogueado.IdEmpresa, idEmpresa, periodo, CancellationToken.None);

                if (!exito || contenido is null || nombreArchivo is null)
                {
                    return new Respuesta { IdTipoMensaje = 1, Mensaje = mensaje };
                }

                return new Respuesta
                {
                    IdTipoMensaje = 2,
                    Mensaje = mensaje,
                    Result = new SireRvieExportacion
                    {
                        NombreArchivo = nombreArchivo,
                        ContentType = "text/plain",
                        Archivo = contenido
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message };
            }
        }

        public async Task<Respuesta> InsertarCampoExtraAsync(UsuarioGeneral usuarioLogueado, int idDocumentoElectronico, string texto)
        {
            try
            {
                var resultado = await _facturacionService.InsertarCampoExtraAsync(
                    new FacturacionInsertarCampoExtraRequest
                    {
                        IdInquilino = usuarioLogueado.IdEmpresa,
                        IdDocumentoElectronico = idDocumentoElectronico,
                        Texto = texto
                    }, CancellationToken.None);

                if (resultado is null || resultado.IdTipoMensaje != 2)
                {
                    return new Respuesta { IdTipoMensaje = resultado?.IdTipoMensaje ?? 3, Mensaje = resultado?.Mensaje ?? "No se pudo registrar el campo extra." };
                }

                return new Respuesta { IdTipoMensaje = 2, Mensaje = "Consulta exitosa.", Result = resultado.Datos };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message };
            }
        }

        public async Task<Respuesta> InsertarLoteCamposExtraAsync(UsuarioGeneral usuarioLogueado, int idDocumentoElectronico, List<FacturacionCampoExtraEntrada> camposExtra)
        {
            try
            {
                var resultado = await _facturacionService.InsertarLoteCamposExtraAsync(
                    new FacturacionInsertarLoteCamposExtraRequest
                    {
                        IdInquilino = usuarioLogueado.IdEmpresa,
                        IdDocumentoElectronico = idDocumentoElectronico,
                        CamposExtra = camposExtra
                    }, CancellationToken.None);

                if (resultado is null || resultado.IdTipoMensaje != 2)
                {
                    return new Respuesta { IdTipoMensaje = resultado?.IdTipoMensaje ?? 3, Mensaje = resultado?.Mensaje ?? "No se pudieron registrar los campos extra." };
                }

                return new Respuesta { IdTipoMensaje = 2, Mensaje = "Consulta exitosa.", Result = resultado.Datos };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message };
            }
        }

        public async Task<Respuesta> ListarCamposExtraAsync(UsuarioGeneral usuarioLogueado, int idDocumentoElectronico)
        {
            try
            {
                var resultado = await _facturacionService.ListarCamposExtraAsync(usuarioLogueado.IdEmpresa, idDocumentoElectronico, CancellationToken.None);

                if (resultado is null || resultado.IdTipoMensaje != 2)
                {
                    return new Respuesta { IdTipoMensaje = resultado?.IdTipoMensaje ?? 3, Mensaje = resultado?.Mensaje ?? "No se pudieron obtener los campos extra." };
                }

                return new Respuesta { IdTipoMensaje = 2, Mensaje = "Consulta exitosa.", Result = resultado.Datos };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message };
            }
        }

        public async Task<Respuesta> ActualizarCampoExtraAsync(UsuarioGeneral usuarioLogueado, int idCampoExtraDocumentoElectronico, string texto)
        {
            try
            {
                var resultado = await _facturacionService.ActualizarCampoExtraAsync(
                    usuarioLogueado.IdEmpresa, idCampoExtraDocumentoElectronico,
                    new FacturacionCampoExtraEntrada { Texto = texto }, CancellationToken.None);

                if (resultado is null || resultado.IdTipoMensaje != 2)
                {
                    return new Respuesta { IdTipoMensaje = resultado?.IdTipoMensaje ?? 3, Mensaje = resultado?.Mensaje ?? "No se pudo actualizar el campo extra." };
                }

                return new Respuesta { IdTipoMensaje = 2, Mensaje = "Consulta exitosa.", Result = resultado.Datos };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message };
            }
        }

        public async Task<Respuesta> EliminarCampoExtraAsync(UsuarioGeneral usuarioLogueado, int idCampoExtraDocumentoElectronico)
        {
            try
            {
                var resultado = await _facturacionService.EliminarCampoExtraAsync(usuarioLogueado.IdEmpresa, idCampoExtraDocumentoElectronico, CancellationToken.None);

                if (resultado is null || resultado.IdTipoMensaje != 2)
                {
                    return new Respuesta { IdTipoMensaje = resultado?.IdTipoMensaje ?? 3, Mensaje = resultado?.Mensaje ?? "No se pudo eliminar el campo extra." };
                }

                return new Respuesta { IdTipoMensaje = 2, Mensaje = "Consulta exitosa.", Result = resultado.Datos };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message };
            }
        }

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
                    TipoCambio = request.tipoCambio,
                    IdTipoOperacionMaestro = request.idTipoOperacionMaestro,
                    FormaPago = new FacturacionFormaPago
                    {
                        IdFormaPago = request.idFormaPago,
                        Cuotas = request.cuotas?.Select(c => new FacturacionCuota
                        {
                            NumeroCuota = c.numeroCuota,
                            FechaVencimiento = c.fechaVencimiento,
                            Monto = c.monto,
                            IdEstadoCuotaMaestro = c.idEstadoCuotaMaestro
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
                    Items = request.lineas.Select((l, i) => new FacturacionItem
                    {
                        NumeroLinea = i + 1,
                        ProductoCodigo = pedidosPorId[l.idPedido].Codigo,
                        ProductoSunatCodigo = l.productoSunatCodigo,
                        Descripcion = !string.IsNullOrWhiteSpace(l.descripcion) ? l.descripcion : pedidosPorId[l.idPedido].NombreCliente ?? string.Empty,
                        IdUnidadMedidaMaestro = l.idUnidadMedidaMaestro,
                        Cantidad = l.cantidad,
                        ValorUnitario = pedidosPorId[l.idPedido].Precio.Value,
                        MontoDescuento = l.montoDescuento,
                        IdAfectacionIgvMaestro = l.idAfectacionIgvMaestro,
                        PorcentajeIgv = l.porcentajeIgv
                    }).ToList(),
                    CamposExtra = request.camposExtra?.Select(c => new FacturacionCampoExtraEntrada { Texto = c.Texto }).ToList()
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

        // A diferencia de GuardarBorradorFacturaAsync (líneas/cliente resueltos desde Pedido/Tarifario/
        // CLIENTES), acá no hay Pedido de por medio: la Nota de Crédito/Débito referencia un documento ya
        // emitido (documentoAfectado, obligatorio) y el front manda cliente e ítems completos. No se
        // registra nada en PEDIDO_FACTURA porque no hay idPedido al que atar el envío.
        public async Task<Respuesta> GenerarNotaCreditoDebitoAsync(UsuarioGeneral usuarioLogueado, GenerarNotaCreditoDebitoRequest request)
        {
            try
            {
                if (request.lineas.Count == 0)
                {
                    return new Respuesta { IdTipoMensaje = 1, Mensaje = "La nota debe tener al menos una línea." };
                }

                var facturacionRequest = new FacturacionInsertarDocumentoRequest
                {
                    IdInquilino = usuarioLogueado.IdEmpresa,
                    IdEmpresa = 1, // TODO: resolver desde EMPRESAS de ms-facturación, mismo TODO que GuardarBorradorFacturaAsync.
                    IdExterno = request.documentoAfectado.idDocumentoElectronicoRelacionado.ToString(),
                    NumeroReferencia = request.numeroReferencia,
                    IdTipoDocumentoMaestro = request.idTipoDocumentoMaestro,
                    IdMonedaMaestro = request.idMonedaMaestro,
                    TipoCambio = request.tipoCambio,
                    IdTipoOperacionMaestro = request.idTipoOperacionMaestro,
                    // Sin FormaPago: una Nota de Crédito/Débito no tiene forma de pago propia (ver
                    // comentario en GenerarNotaCreditoDebitoRequest).
                    Cliente = new FacturacionCliente
                    {
                        IdTipoDocumentoSunat = request.cliente.idTipoDocumentoSunat,
                        NumeroDocumento = request.cliente.numeroDocumento,
                        Nombre = request.cliente.nombre,
                        Correo = request.cliente.correo,
                        Direccion = request.cliente.direccion,
                        PaisCodigo = request.cliente.paisCodigo
                    },
                    DocumentoAfectado = new FacturacionDocumentoAfectado
                    {
                        IdDocumentoElectronicoRelacionado = request.documentoAfectado.idDocumentoElectronicoRelacionado,
                        IdMotivoMaestro = request.documentoAfectado.idMotivoMaestro
                    },
                    Items = request.lineas.Select((l, i) => new FacturacionItem
                    {
                        NumeroLinea = i + 1,
                        ProductoCodigo = l.productoCodigo,
                        ProductoSunatCodigo = l.productoSunatCodigo,
                        Descripcion = l.descripcion,
                        IdUnidadMedidaMaestro = l.idUnidadMedidaMaestro,
                        Cantidad = l.cantidad,
                        ValorUnitario = l.valorUnitario,
                        MontoDescuento = l.montoDescuento,
                        IdAfectacionIgvMaestro = l.idAfectacionIgvMaestro,
                        PorcentajeIgv = l.porcentajeIgv
                    }).ToList(),
                    CamposExtra = request.camposExtra?.Select(c => new FacturacionCampoExtraEntrada { Texto = c.Texto }).ToList()
                };

                var insertado = await _facturacionService.InsertarDocumentoAsync(facturacionRequest, CancellationToken.None);
                if (insertado is null || insertado.IdTipoMensaje != 2 || insertado.Datos is null)
                {
                    return new Respuesta { IdTipoMensaje = insertado?.IdTipoMensaje ?? 3, Mensaje = insertado?.Mensaje ?? "No se pudo crear el documento electrónico en facturación." };
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
                    TipoCambio = request.tipoCambio,
                    IdTipoOperacionMaestro = request.idTipoOperacionMaestro,
                    Lineas = request.lineas.Select((l, i) => new FacturacionLineaEdicion
                    {
                        NumeroLinea = i + 1,
                        ProductoCodigo = pedidosPorId[l.idPedido].Codigo,
                        ProductoSunatCodigo = l.productoSunatCodigo,
                        Descripcion = !string.IsNullOrWhiteSpace(l.descripcion) ? l.descripcion : pedidosPorId[l.idPedido].NombreCliente ?? string.Empty,
                        IdUnidadMedidaMaestro = l.idUnidadMedidaMaestro,
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
                        IdCuotaDocumentoElectronico = c.idCuotaDocumentoElectronico,
                        IdEstadoCuotaMaestro = c.idEstadoCuotaMaestro
                    }).ToList(),
                    CamposExtra = request.camposExtra?.Select(c => new FacturacionCampoExtraEdicion
                    {
                        Texto = c.Texto,
                        IdCampoExtraDocumentoElectronico = c.idCampoExtraDocumentoElectronico
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

        // Edita una Nota de Crédito/Débito existente en PendienteEnvio — mismo criterio que
        // GenerarNotaCreditoDebitoAsync frente a GuardarBorradorFacturaAsync: sin idPedido, líneas completas
        // tal cual las manda el front, sin registrar/reconciliar nada en PEDIDO_FACTURA.
        public async Task<Respuesta> EditarNotaCreditoDebitoAsync(
            UsuarioGeneral usuarioLogueado, int idDocumentoElectronico, EditarNotaCreditoDebitoRequest request)
        {
            try
            {
                if (request.lineas.Count == 0)
                {
                    return new Respuesta { IdTipoMensaje = 1, Mensaje = "La nota debe tener al menos una línea." };
                }

                var facturacionRequest = new FacturacionGuardarCambiosRequest
                {
                    // Sin IdFormaPago/Cuotas: una Nota de Crédito/Débito no tiene forma de pago propia (ver
                    // comentario en EditarNotaCreditoDebitoRequest).
                    NumeroReferencia = request.numeroReferencia,
                    IdMonedaMaestro = request.idMonedaMaestro,
                    TipoCambio = request.tipoCambio,
                    IdTipoOperacionMaestro = request.idTipoOperacionMaestro,
                    IdMotivoMaestro = request.idMotivoMaestro,
                    Lineas = request.lineas.Select((l, i) => new FacturacionLineaEdicion
                    {
                        NumeroLinea = i + 1,
                        ProductoCodigo = l.productoCodigo,
                        ProductoSunatCodigo = l.productoSunatCodigo,
                        Descripcion = l.descripcion,
                        IdUnidadMedidaMaestro = l.idUnidadMedidaMaestro,
                        Cantidad = l.cantidad,
                        ValorUnitario = l.valorUnitario,
                        MontoDescuento = l.montoDescuento,
                        IdAfectacionIgvMaestro = l.idAfectacionIgvMaestro,
                        PorcentajeIgv = l.porcentajeIgv,
                        IdLineaDocumentoElectronico = l.idLineaDocumentoElectronico
                    }).ToList(),
                    CamposExtra = request.camposExtra?.Select(c => new FacturacionCampoExtraEdicion
                    {
                        Texto = c.Texto,
                        IdCampoExtraDocumentoElectronico = c.idCampoExtraDocumentoElectronico
                    }).ToList()
                };

                var resultado = await _facturacionService.GuardarCambiosAsync(
                    usuarioLogueado.IdEmpresa, idDocumentoElectronico, facturacionRequest, CancellationToken.None);

                if (resultado is null || resultado.IdTipoMensaje != 2)
                {
                    return new Respuesta { IdTipoMensaje = resultado?.IdTipoMensaje ?? 3, Mensaje = resultado?.Mensaje ?? "No se pudieron guardar los cambios en facturación." };
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
        // EstadoMaestroCodigo (ms-facturación) → PEDIDO_FACTURA.IdEstadoFacturacion (TABLA_MAESTRA IdMaestro=68).
        // Error (8) no mapea: es una falla de transmisión, no una decisión de SUNAT — el pedido se queda en
        // Borrador Factura (10) para poder reintentar el envío.
        private static int? MapearEstadoFacturacion(int estadoCodigoSunat) => estadoCodigoSunat switch
        {
            3 or 4 => 5, // Aceptado / AceptadoConObservaciones → Aprobado
            5 => 6,      // Rechazado → Rechazado
            _ => null
        };

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

                var idEstadoFacturacion = MapearEstadoFacturacion(resultado.Datos.EstadoCodigo);
                if (idEstadoFacturacion.HasValue)
                {
                    var actualizacion = await _pedidoFacturaDao.ActualizarEstadoPorDocumentoAsync(
                        usuarioLogueado.IdEmpresa, [(idDocumentoElectronico, idEstadoFacturacion.Value)]);

                    if (actualizacion.IdTipoMensaje != 2)
                    {
                        _logger.LogWarning(
                            "No se pudo actualizar el estado de facturación del documento {IdDocumentoElectronico} tras el envío a SUNAT: {Mensaje}",
                            idDocumentoElectronico, actualizacion.Mensaje);
                    }
                }

                return new Respuesta { IdTipoMensaje = 2, Mensaje = "Documento emitido correctamente.", Result = resultado.Datos };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message };
            }
        }

        // Dispara la Comunicación de Baja (sendSummary) para los documentos indicados. sendSummary nunca
        // resuelve en la misma llamada — éxito acá es "el ticket se generó", no "SUNAT aceptó la anulación".
        // El veredicto real llega después vía SincronizacionFacturacionWorker, que lleva los pedidos a
        // Anulación Aprobada (8) o Anulación Rechazada (9). Acá solo se los marca Pendiente Anulación (7).
        public async Task<Respuesta> AnularFacturasAsync(UsuarioGeneral usuarioLogueado, AnularFacturasRequest request)
        {
            try
            {
                if (request.Items.Count == 0)
                {
                    return new Respuesta { IdTipoMensaje = 1, Mensaje = "Debe indicar al menos un documento a anular." };
                }

                var facturacionRequest = new FacturacionComunicacionBajaRequest
                {
                    IdInquilino = usuarioLogueado.IdEmpresa,
                    IdEmpresa = 1, // TODO: resolver desde EMPRESAS de ms-facturación (GET /api/v1/empresas?idInquilino=) en vez de fijo.
                    FechaReferencia = request.FechaReferencia,
                    Items = request.Items
                        .Select(item => new FacturacionItemBaja { IdDocumentoElectronico = item.IdDocumentoElectronico, MotivoDescripcion = item.MotivoDescripcion })
                        .ToList()
                };

                var resultado = await _facturacionService.EnviarComunicacionBajaAsync(facturacionRequest, CancellationToken.None);

                if (resultado is null || resultado.IdTipoMensaje != 2 || resultado.Datos is null)
                {
                    return new Respuesta { IdTipoMensaje = resultado?.IdTipoMensaje ?? 3, Mensaje = resultado?.Mensaje ?? "No se pudo enviar la comunicación de baja." };
                }

                var documentosConEstado = request.Items
                    .Select(item => (item.IdDocumentoElectronico, IdEstadoFacturacion: 7)) // Pendiente Anulación
                    .ToList();

                var actualizacion = await _pedidoFacturaDao.ActualizarEstadoPorDocumentoAsync(usuarioLogueado.IdEmpresa, documentosConEstado);
                if (actualizacion.IdTipoMensaje != 2)
                {
                    _logger.LogWarning(
                        "No se pudo marcar Pendiente Anulación los documentos {IdDocumentos} tras enviar la comunicación de baja: {Mensaje}",
                        string.Join(",", request.Items.Select(i => i.IdDocumentoElectronico)), actualizacion.Mensaje);
                }

                return new Respuesta { IdTipoMensaje = 2, Mensaje = "Comunicación de baja enviada correctamente.", Result = resultado.Datos };
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

        // Rol validado acá (SP_PedidoFactura_Resumen, maximilian_staging); el monto real viene de
        // ms-facturación (SP_DocumentoElectronico_ObtenerResumenFacturacion) — mismo patrón de dos pasos
        // que el resto de este Handler, nunca acceso directo entre bases de datos.
        public async Task<Respuesta> ObtenerResumenDashboardAsync(UsuarioGeneral usuarioLogueado, DateOnly? fechaDesde, DateOnly? fechaHasta)
        {
            try
            {
                var acceso = await _pedidoFacturaDao.ValidarAccesoResumenAsync(usuarioLogueado);
                if (acceso.IdTipoMensaje != 2)
                {
                    return acceso;
                }

                // Mismos valores por defecto que tenía SP_PedidoFactura_Resumen antes de recortarse: sin
                // rango explícito, el dashboard muestra el mes calendario anterior hasta hoy.
                var desde = fechaDesde ?? new DateOnly(DateTime.Today.AddMonths(-1).Year, DateTime.Today.AddMonths(-1).Month, 1);
                var hasta = fechaHasta ?? DateOnly.FromDateTime(DateTime.Today);

                if (desde >= hasta)
                {
                    return new Respuesta { IdTipoMensaje = 1, Mensaje = "La fecha desde no puede ser mayor o igual a la fecha hasta." };
                }

                var resultado = await _facturacionService.ObtenerResumenAsync(
                    usuarioLogueado.IdEmpresa, // IdInquilino en ms-facturación = IdEmpresa acá
                    1, // TODO: resolver desde EMPRESAS de ms-facturación (GET /api/v1/empresas?idInquilino=) en vez de fijo.
                    desde, hasta, CancellationToken.None);

                if (resultado is null || resultado.IdTipoMensaje != 2 || resultado.Datos is null)
                {
                    return new Respuesta { IdTipoMensaje = resultado?.IdTipoMensaje ?? 3, Mensaje = resultado?.Mensaje ?? "No se pudo obtener el resumen de facturación." };
                }

                return new Respuesta
                {
                    IdTipoMensaje = 2,
                    Mensaje = "Resumen de pedidos facturados generado correctamente.",
                    Result = new ResumenPedidoFacturaConsulta
                    {
                        FechaDesde = desde,
                        FechaHasta = hasta,
                        MontoTotalMensual = resultado.Datos.MontoTotalPEN,
                        CantidadFacturasEmitidas = resultado.Datos.CantidadFacturas,
                        PromedioIngresoMensual = resultado.Datos.PromedioIngresoPEN,
                        MonedaIcono = resultado.Datos.MonedaIcono
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message };
            }
        }
    }
}
