using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SafetyReport.DAO;
using SafetyReport.Models;
using System.Globalization;
using System.Text;

namespace SafetyReport.Handlers
{
    public class PedidoFacturaHandler
    {
        private readonly PedidoFacturaDAO _pedidoFacturaDao;
        private readonly PedidoFacturaLineaDAO _pedidoFacturaLineaDao;
        private readonly PedidoDAO _pedidoDao;
        private readonly ClienteDAO _clienteDao;
        private readonly FacturacionElectronicaService _facturacionService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PedidoFacturaHandler> _logger;

        public PedidoFacturaHandler(
            PedidoFacturaDAO pedidoFacturaDao, PedidoFacturaLineaDAO pedidoFacturaLineaDao, PedidoDAO pedidoDao, ClienteDAO clienteDao,
            FacturacionElectronicaService facturacionService, IConfiguration configuration, ILogger<PedidoFacturaHandler> logger)
        {
            _pedidoFacturaDao = pedidoFacturaDao;
            _pedidoFacturaLineaDao = pedidoFacturaLineaDao;
            _pedidoDao = pedidoDao;
            _clienteDao = clienteDao;
            _facturacionService = facturacionService;
            _configuration = configuration;
            _logger = logger;
        }

        public Task<Respuesta> ListarPedidosParaFacturacionAsync(UsuarioGeneral usuarioLogueado, ListarPedidosFacturacionRequest request) =>
            _pedidoDao.ListarParaFacturacionAsync(usuarioLogueado, request);

        // El CRUD de líneas (crear/editar/listar/desvincular manual) vive en PedidoFacturaLineaHandler.
        // Acá se queda todo lo que opera sobre documentos/pedidos y solo referencia IdPedidoFacturaLinea
        // de paso (p. ej. RegistrarEnvioAsync más abajo, que asocia líneas ya existentes a un documento).

        // Genera el Excel de SP_Pedido_ListarParaPrefactura. El nombre de cliente para el nombre de archivo
        // se resuelve aparte (ClienteDAO) en vez de tomarlo de la primera fila — así el nombre del archivo
        // no depende de que existan pedidos en el rango, y evita quedar vacío si el filtro no matchea nada.
        public async Task<Respuesta> ExportarPedidosParaPrefacturaAsync(UsuarioGeneral usuarioLogueado, FiltroPedidoPrefactura request)
        {
            try
            {
                var respuesta = await _pedidoDao.ListarParaPrefacturaAsync(usuarioLogueado, request);
                if (respuesta.IdTipoMensaje != 2)
                    return respuesta;

                var items = respuesta.Result as List<PedidoPrefacturaConsulta> ?? new();

                var clienteResp = await _clienteDao.ObtenerClienteAsync(usuarioLogueado, request.IdCliente);
                var nombreCliente = clienteResp.IdTipoMensaje == 2
                    ? (clienteResp.Result as List<ClienteConsulta>)?.FirstOrDefault()?.Nombre
                    : null;
                nombreCliente = string.IsNullOrWhiteSpace(nombreCliente) ? "CLIENTE" : nombreCliente;

                var archivo = GenerarExcelPrefactura(items);
                var etiquetaPeriodo = ObtenerEtiquetaPeriodo(request);
                var nombreArchivo = $"{SanitizarNombreArchivo(nombreCliente)} List of Reports {etiquetaPeriodo}.xlsx";

                respuesta.Result = new PedidoPrefacturaExportacion
                {
                    NombreArchivo = nombreArchivo,
                    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    Archivo = archivo
                };
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = null! };
            }
        }

        // Por mes(es) -> "July 2026" o "July 2026, September 2026" (uno o varios, no necesariamente
        // contiguos); por rango explícito -> "06 July 2026 - 15 August 2026". Solo se llega acá con
        // IdTipoMensaje=2, así que el SP ya garantizó que exactamente uno de los dos vino.
        private static string ObtenerEtiquetaPeriodo(FiltroPedidoPrefactura request)
        {
            if (request.Meses is { Count: > 0 })
            {
                var etiquetas = request.Meses.Select(am =>
                    new DateOnly(am.Anio, am.Mes, 1).ToString("MMMM yyyy", CultureInfo.InvariantCulture));
                return string.Join(", ", etiquetas);
            }

            var fchInicioTexto = request.FchInicio?.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture) ?? string.Empty;
            var fchFinTexto = request.FchFin?.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture) ?? string.Empty;
            return $"{fchInicioTexto} - {fchFinTexto}";
        }

        // Mismos caracteres inválidos en Windows/macOS/Linux (\ / : * ? " < > |) más los de control — el
        // nombre de cliente puede traer puntos/paréntesis (v.g. "S.A. (RAZÓN)"), esos sí son válidos.
        // Tildes/ñ se quitan aparte (EliminarDiacriticos) — solo afecta el nombre de archivo, el contenido
        // del Excel (columna CLIENT, etc.) sigue con el nombre tal cual viene de la base.
        private static string SanitizarNombreArchivo(string valor)
        {
            var sinDiacriticos = EliminarDiacriticos(valor);
            var caracteresInvalidos = Path.GetInvalidFileNameChars();
            var limpio = new string(sinDiacriticos.Where(c => !caracteresInvalidos.Contains(c)).ToArray()).Trim();

            return string.IsNullOrWhiteSpace(limpio) ? "CLIENTE" : limpio;
        }

        // NFD descompone "á"->"a"+´, "ñ"->"n"+~; descartar los Non-Spacing Marks deja solo la letra base.
        // El acento suelto (´, U+00B4) no cuelga de ninguna letra, así que se quita aparte antes de normalizar.
        private static string EliminarDiacriticos(string valor)
        {
            var sinAcentoSuelto = valor.Replace("´", "").Replace("`", "");
            var normalizado = sinAcentoSuelto.Normalize(NormalizationForm.FormD);
            var sinMarcas = normalizado.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark);

            return new string(sinMarcas.ToArray()).Normalize(NormalizationForm.FormC);
        }

        private static byte[] GenerarExcelPrefactura(List<PedidoPrefacturaConsulta> items)
        {
            using var stream = new MemoryStream();
            using (var document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
            {
                var workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                var sheetData = new SheetData();
                worksheetPart.Worksheet = new Worksheet();
                worksheetPart.Worksheet.Append(sheetData);

                var sheets = workbookPart.Workbook.AppendChild(new Sheets());
                sheets.Append(new Sheet
                {
                    Id = workbookPart.GetIdOfPart(worksheetPart),
                    SheetId = 1,
                    Name = "Reports"
                });

                sheetData.Append(CrearFilaExcelPrefactura(
                    "CLIENT", "COMPANY", "TYPE OF REPORT", "REFERENCE NO.", "COUNTRY", "DATE OF REQUEST", "CURRENCY", "PRICE"
                    ));

                foreach (var item in items)
                {
                    sheetData.Append(CrearFilaExcelPrefactura(
                        item.Client, item.Company, item.TypeOfReport, item.ReferenceNo, item.Country, item.DateOfRequest,
                        item.Currency, item.Price.ToString("F2", CultureInfo.InvariantCulture)
                        ));
                }

                workbookPart.Workbook.Save();
            }

            return stream.ToArray();
        }

        private static Row CrearFilaExcelPrefactura(params string?[] valores)
        {
            var row = new Row();
            foreach (var valor in valores)
                row.Append(new Cell
                {
                    DataType = CellValues.InlineString,
                    InlineString = new InlineString(new Text(valor ?? string.Empty))
                });
            return row;
        }

        // Listado de facturas ya generadas — NumeroFactura/ClienteNombre/FormaPago/Estado vienen resueltos
        // por ms-facturación, este Handler solo hace de proxy con los filtros del front.
        public async Task<Respuesta> ListarFacturasAsync(UsuarioGeneral usuarioLogueado, ListarFacturasRequest request)
        {
            try
            {
                var acceso = await _pedidoFacturaDao.ValidarAccesoFacturacionAsync(usuarioLogueado, "listar las facturas");
                if (acceso.IdTipoMensaje != 2)
                {
                    return acceso;
                }

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
                var acceso = await _pedidoFacturaDao.ValidarAccesoFacturacionAsync(usuarioLogueado, "obtener la factura");
                if (acceso.IdTipoMensaje != 2)
                {
                    return acceso;
                }

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
                var acceso = await _pedidoFacturaDao.ValidarAccesoFacturacionAsync(usuarioLogueado, "obtener los datos para la nota");
                if (acceso.IdTipoMensaje != 2)
                {
                    return acceso;
                }

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
                var acceso = await _pedidoFacturaDao.ValidarAccesoFacturacionAsync(usuarioLogueado, "obtener el link de verificación");
                if (acceso.IdTipoMensaje != 2)
                {
                    return acceso;
                }

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
                var acceso = await _pedidoFacturaDao.ValidarAccesoFacturacionAsync(usuarioLogueado, "obtener la url de descarga");
                if (acceso.IdTipoMensaje != 2)
                {
                    return acceso;
                }

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
                var acceso = await _pedidoFacturaDao.ValidarAccesoFacturacionAsync(usuarioLogueado, "obtener los errores del último envío");
                if (acceso.IdTipoMensaje != 2)
                {
                    return acceso;
                }

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
                var acceso = await _pedidoFacturaDao.ValidarAccesoFacturacionAsync(usuarioLogueado, "generar el TXT SIRE RVIE");
                if (acceso.IdTipoMensaje != 2)
                {
                    return acceso;
                }

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
                var acceso = await _pedidoFacturaDao.ValidarAccesoFacturacionAsync(usuarioLogueado, "insertar el campo extra");
                if (acceso.IdTipoMensaje != 2)
                {
                    return acceso;
                }

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

        public async Task<Respuesta> ActualizarEstadoCuotaAsync(
            UsuarioGeneral usuarioLogueado, int idDocumentoElectronico, int idCuotaDocumentoElectronico,
            int idEstadoCuotaMaestro, DateTime? fechaPago)
        {
            try
            {
                var acceso = await _pedidoFacturaDao.ValidarAccesoFacturacionAsync(usuarioLogueado, "actualizar el estado de la cuota");
                if (acceso.IdTipoMensaje != 2)
                {
                    return acceso;
                }

                var resultado = await _facturacionService.ActualizarEstadoCuotaAsync(
                    usuarioLogueado.IdEmpresa, idDocumentoElectronico, idCuotaDocumentoElectronico, idEstadoCuotaMaestro, fechaPago, CancellationToken.None);

                if (resultado is null || resultado.IdTipoMensaje != 2)
                {
                    return new Respuesta { IdTipoMensaje = resultado?.IdTipoMensaje ?? 3, Mensaje = resultado?.Mensaje ?? "No se pudo actualizar el estado de la cuota." };
                }

                return new Respuesta { IdTipoMensaje = 2, Mensaje = "Consulta exitosa.", Result = resultado.Datos };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message };
            }
        }

        // Result es un arreglo (un item por documento afectado: el indicado + toda Nota de Crédito/Débito
        // vigente arrastrada automáticamente con él), no un solo objeto — ver
        // FacturacionElectronicaService.AnularManualmenteAsync.
        public async Task<Respuesta> AnularManualmenteAsync(
            UsuarioGeneral usuarioLogueado, int idDocumentoElectronico, string motivo, DateTime fechaAnulacion)
        {
            try
            {
                var acceso = await _pedidoFacturaDao.ValidarAccesoFacturacionAsync(usuarioLogueado, "anular manualmente el documento");
                if (acceso.IdTipoMensaje != 2)
                {
                    return acceso;
                }

                var resultado = await _facturacionService.AnularManualmenteAsync(
                    usuarioLogueado.IdEmpresa, idDocumentoElectronico,
                    new FacturacionAnularManualmenteRequest { Motivo = motivo, FechaAnulacion = fechaAnulacion },
                    CancellationToken.None);

                if (resultado is null || resultado.IdTipoMensaje != 2)
                {
                    return new Respuesta { IdTipoMensaje = resultado?.IdTipoMensaje ?? 3, Mensaje = resultado?.Mensaje ?? "No se pudo registrar la anulación manual." };
                }

                // A diferencia de la Comunicación de Baja real (actualizada por SincronizacionFacturacionWorker
                // al resolverse el ticket), acá no hay un worker async — se libera el pedido en el mismo
                // request. No falla la operación si esto falla: el documento ya quedó anulado en ms-facturación,
                // solo queda desincronizado el vínculo (mismo criterio que GuardarBorradorFacturaAsync).
                if (resultado.Datos is { Count: > 0 })
                {
                    var liberacion = await _pedidoFacturaDao.ActualizarEstadoPorDocumentoAsync(
                        usuarioLogueado.IdEmpresa,
                        resultado.Datos.Select(d => (d.IdDocumentoElectronico, IdEstadoFacturacion: 15)).ToList()); // AnuladoManualmente
                    if (liberacion.IdTipoMensaje != 2)
                    {
                        _logger.LogWarning(
                            "No se pudo liberar los pedidos de los documentos anulados manualmente {IdsDocumento}: {Mensaje}",
                            string.Join(",", resultado.Datos.Select(d => d.IdDocumentoElectronico)), liberacion.Mensaje);
                    }
                }

                return new Respuesta { IdTipoMensaje = 2, Mensaje = "Consulta exitosa.", Result = resultado.Datos };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message };
            }
        }

        // Elimina un borrador que nunca se envió a SUNAT (PendienteEnvio) y libera los pedidos que seguían
        // enlazados a él — para Notas de Crédito/Débito la liberación es un no-op (ninguna fila de
        // PEDIDO_FACTURA apunta a su id), mismo criterio que AnularManualmenteAsync.
        public async Task<Respuesta> EliminarBorradorFacturaAsync(UsuarioGeneral usuarioLogueado, int idDocumentoElectronico)
        {
            try
            {
                var acceso = await _pedidoFacturaDao.ValidarAccesoFacturacionAsync(usuarioLogueado, "eliminar el borrador");
                if (acceso.IdTipoMensaje != 2)
                {
                    return acceso;
                }

                var resultado = await _facturacionService.EliminarBorradorAsync(
                    usuarioLogueado.IdEmpresa, idDocumentoElectronico, CancellationToken.None);

                if (resultado is null || resultado.IdTipoMensaje != 2)
                {
                    return new Respuesta { IdTipoMensaje = resultado?.IdTipoMensaje ?? 3, Mensaje = resultado?.Mensaje ?? "No se pudo eliminar el borrador." };
                }

                var liberacion = await _pedidoFacturaDao.DesvincularAsync(usuarioLogueado, idDocumentoElectronico, []);
                if (liberacion.IdTipoMensaje != 2)
                {
                    _logger.LogWarning(
                        "No se pudo liberar los pedidos del borrador eliminado {IdDocumentoElectronico}: {Mensaje}",
                        idDocumentoElectronico, liberacion.Mensaje);
                }

                return new Respuesta { IdTipoMensaje = 2, Mensaje = "Borrador eliminado correctamente." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message };
            }
        }

        public async Task<Respuesta> PrevisualizarAnulacionManualAsync(UsuarioGeneral usuarioLogueado, int idDocumentoElectronico)
        {
            try
            {
                var acceso = await _pedidoFacturaDao.ValidarAccesoFacturacionAsync(usuarioLogueado, "previsualizar la anulación manual");
                if (acceso.IdTipoMensaje != 2)
                {
                    return acceso;
                }

                var resultado = await _facturacionService.PrevisualizarAnulacionManualAsync(
                    usuarioLogueado.IdEmpresa, idDocumentoElectronico, CancellationToken.None);

                if (resultado is null || resultado.IdTipoMensaje != 2)
                {
                    return new Respuesta { IdTipoMensaje = resultado?.IdTipoMensaje ?? 3, Mensaje = resultado?.Mensaje ?? "No se pudo previsualizar la anulación manual." };
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
                var acceso = await _pedidoFacturaDao.ValidarAccesoFacturacionAsync(usuarioLogueado, "insertar el lote de campos extra");
                if (acceso.IdTipoMensaje != 2)
                {
                    return acceso;
                }

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
                var acceso = await _pedidoFacturaDao.ValidarAccesoFacturacionAsync(usuarioLogueado, "listar los campos extra");
                if (acceso.IdTipoMensaje != 2)
                {
                    return acceso;
                }

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
                var acceso = await _pedidoFacturaDao.ValidarAccesoFacturacionAsync(usuarioLogueado, "actualizar el campo extra");
                if (acceso.IdTipoMensaje != 2)
                {
                    return acceso;
                }

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
                var acceso = await _pedidoFacturaDao.ValidarAccesoFacturacionAsync(usuarioLogueado, "eliminar el campo extra");
                if (acceso.IdTipoMensaje != 2)
                {
                    return acceso;
                }

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
                var acceso = await _pedidoFacturaDao.ValidarAccesoFacturacionAsync(usuarioLogueado, "obtener la factura por pedido");
                if (acceso.IdTipoMensaje != 2)
                {
                    return acceso;
                }

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
                var acceso = await _pedidoFacturaDao.ValidarAccesoFacturacionAsync(usuarioLogueado, "guardar el borrador de la factura");
                if (acceso.IdTipoMensaje != 2)
                {
                    return acceso;
                }

                if (request.lineas.Count == 0)
                {
                    return new Respuesta { IdTipoMensaje = 1, Mensaje = "La factura debe tener al menos una línea." };
                }

                var idsLinea = request.lineas.Select(l => l.idPedidoFacturaLinea).Distinct().ToList();

                var lineasResp = await _pedidoFacturaLineaDao.ObtenerParaBorradorAsync(usuarioLogueado, request.idCliente, idsLinea);
                if (lineasResp.IdTipoMensaje != 2 || lineasResp.Result is not LineasParaBorradorConsulta lineasData)
                {
                    return new Respuesta { IdTipoMensaje = lineasResp.IdTipoMensaje, Mensaje = lineasResp.Mensaje };
                }

                var lineasPorId = lineasData.Lineas.ToDictionary(l => l.IdPedidoFacturaLinea);

                var idsLineaFaltantes = idsLinea.Where(id => !lineasPorId.ContainsKey(id)).ToList();
                if (idsLineaFaltantes.Count > 0)
                {
                    return new Respuesta
                    {
                        IdTipoMensaje = 1,
                        Mensaje = $"Una o más líneas no están disponibles para el borrador: {string.Join(",", idsLineaFaltantes)}."
                    };
                }

                // Cliente se resuelve directo por idCliente (SP_Cliente_Obtener) — ya trae
                // IdTipoDocumentoSunat/NumRegistroTributario, no hace falta pasar por pedidos.
                var clienteResp = await _clienteDao.ObtenerClienteAsync(usuarioLogueado, request.idCliente);
                var clienteDatos = clienteResp.IdTipoMensaje == 2
                    ? (clienteResp.Result as List<ClienteConsulta>)?.FirstOrDefault()
                    : null;

                if (clienteDatos is null || clienteDatos.IdTipoDocumentoSunat is null)
                {
                    return new Respuesta { IdTipoMensaje = 1, Mensaje = "El cliente no tiene un tipo de documento SUNAT configurado." };
                }

                var facturacionRequest = new FacturacionInsertarDocumentoRequest
                {
                    IdInquilino = usuarioLogueado.IdEmpresa,
                    IdEmpresa = 1, // TODO: resolver desde EMPRESAS de ms-facturación (GET /api/v1/empresas?idInquilino=) en vez de fijo.
                    IdExterno = string.Join(",", idsLinea),
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
                            IdEstadoCuotaMaestro = c.idEstadoCuotaMaestro,
                            FechaPago = c.fechaPago
                        }).ToList()
                    },
                    Cliente = new FacturacionCliente
                    {
                        IdTipoDocumentoSunat = clienteDatos.IdTipoDocumentoSunat.Value,
                        NumeroDocumento = clienteDatos.NumRegistroTributario ?? string.Empty,
                        Nombre = clienteDatos.Nombre,
                        Correo = clienteDatos.Correo,
                        Direccion = clienteDatos.Direccion,
                        PaisCodigo = clienteDatos.IdPais
                    },
                    Items = request.lineas.Select((l, i) => new FacturacionItem
                    {
                        NumeroLinea = i + 1,
                        ProductoCodigo = lineasPorId[l.idPedidoFacturaLinea].Codigo ?? string.Empty,
                        ProductoSunatCodigo = l.productoSunatCodigo,
                        Descripcion = lineasPorId[l.idPedidoFacturaLinea].Descripcion,
                        IdUnidadMedidaMaestro = l.idUnidadMedidaMaestro,
                        Cantidad = lineasPorId[l.idPedidoFacturaLinea].Cantidad,
                        ValorUnitario = lineasPorId[l.idPedidoFacturaLinea].ValorUnitario,
                        MontoDescuento = lineasPorId[l.idPedidoFacturaLinea].Descuento,
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

                // Asocia el documento a las líneas (no a los pedidos directamente) — SP_PedidoFactura_
                // RegistrarEnvio fija IdDocumentoElectronico + IdEstadoFacturacion=1 en cada línea.
                var registro = await _pedidoFacturaDao.RegistrarEnvioAsync(
                    usuarioLogueado, idsLinea, insertado.Datos.IdDocumentoElectronico);

                if (registro.IdTipoMensaje != 2)
                {
                    _logger.LogWarning(
                        "No se pudo registrar el borrador de facturación para las líneas {IdsLinea}: {Mensaje}",
                        string.Join(",", idsLinea), registro.Mensaje);
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
                var acceso = await _pedidoFacturaDao.ValidarAccesoFacturacionAsync(usuarioLogueado, "generar la nota de crédito/débito");
                if (acceso.IdTipoMensaje != 2)
                {
                    return acceso;
                }

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
                var acceso = await _pedidoFacturaDao.ValidarAccesoFacturacionAsync(usuarioLogueado, "guardar los cambios de la factura");
                if (acceso.IdTipoMensaje != 2)
                {
                    return acceso;
                }

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
                    // Mismo cálculo que InsertarAsync — se manda de nuevo porque las líneas pudieron cambiar
                    // (IdExterno solo se llenaba al crear el documento y quedaba obsoleto después).
                    IdExterno = string.Join(",", request.lineas.Select(l => l.idPedido)),
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
                        IdEstadoCuotaMaestro = c.idEstadoCuotaMaestro,
                        FechaPago = c.fechaPago
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
                    usuarioLogueado, idPedidos, idDocumentoElectronico);
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
                var acceso = await _pedidoFacturaDao.ValidarAccesoFacturacionAsync(usuarioLogueado, "editar la nota de crédito/débito");
                if (acceso.IdTipoMensaje != 2)
                {
                    return acceso;
                }

                if (request.lineas.Count == 0)
                {
                    return new Respuesta { IdTipoMensaje = 1, Mensaje = "La nota debe tener al menos una línea." };
                }

                var facturacionRequest = new FacturacionGuardarCambiosRequest
                {
                    // Mismo cálculo que GenerarNotaCreditoDebitoAsync — el documento afectado no cambia,
                    // pero igual se recalcula desde el campo del request en vez de asumir que sigue igual.
                    IdExterno = request.idDocumentoElectronicoRelacionado.ToString(),
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
        // PEDIDO_FACTURA_LINEA.IdEstadoFacturacion usa el dominio SUNAT/ms-facturación directamente
        // (EstadoMaestroCodigo, TABLA_MAESTRA IdMaestro=1) — ya no hay traducción a un dominio propio.
        // Error (8) es SUNAT rechazando el contenido del comprobante (dato inválido, no un fallo de
        // transmisión) — antes no mapeaba y el pedido quedaba varado en Borrador Factura, invisible en
        // SP_Pedido_ListarParaFacturacion. Se deja pasar para que vuelva a aparecer.
        private static int? MapearEstadoFacturacion(int estadoCodigoSunat) => estadoCodigoSunat switch
        {
            3 or 4 or 5 or 8 => estadoCodigoSunat, // Aceptado / AceptadoConObservaciones / Rechazado / ErrorSunat
            _ => null
        };

        public async Task<Respuesta> EmitirFacturaAsync(UsuarioGeneral usuarioLogueado, int idDocumentoElectronico)
        {
            try
            {
                var acceso = await _pedidoFacturaDao.ValidarAccesoFacturacionAsync(usuarioLogueado, "emitir la factura");
                if (acceso.IdTipoMensaje != 2)
                {
                    return acceso;
                }

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

        // Dispara la Comunicación de Baja (sendSummary) para los documentos indicados — o el Resumen Diario
        // de Baja si son Boleta, ms-facturación exige mecanismos distintos según el tipo (ver
        // SP_LoteDocumento_Insertar/SP_LoteResumenBajaBoleta_Insertar). Se resuelve el tipo de cada ítem
        // primero (ObtenerTipoDocumentoAsync, en paralelo) y se despacha cada grupo a su endpoint — la
        // mayoría de las veces request.Items trae un solo documento (la UI anula de a uno), así que en la
        // práctica esto casi siempre termina llamando un solo endpoint, pero se resuelve por grupo para no
        // asumirlo. sendSummary nunca resuelve en la misma llamada — éxito acá es "el ticket se generó", no
        // "SUNAT aceptó la anulación". El veredicto real llega después vía SincronizacionFacturacionWorker,
        // que lleva los pedidos a Anulación Aprobada (8) o Anulación Rechazada (9). Acá solo se los marca
        // Pendiente Anulación (7), y solo los documentos cuyo grupo sí se pudo enviar.
        // Boleta directa, o Nota de Crédito/Débito emitida contra una Boleta (Referencia.TipoDocumentoRelacionadoCodigo,
        // grabado una sola vez al crear la Nota — ver SP_DocumentoElectronico_Insertar) — ambas van por Resumen
        // Diario de Baja, nunca por Comunicación de Baja (ms-facturación la rechaza si se intenta).
        private static bool EsBoleta(string? tipoDocumentoCodigo, FacturacionReferenciaTipoLookup? referencia) =>
            tipoDocumentoCodigo == "03" || referencia?.TipoDocumentoRelacionadoCodigo == "03";

        public async Task<Respuesta> AnularFacturasAsync(UsuarioGeneral usuarioLogueado, AnularFacturasRequest request)
        {
            try
            {
                var acceso = await _pedidoFacturaDao.ValidarAccesoFacturacionAsync(usuarioLogueado, "anular las facturas");
                if (acceso.IdTipoMensaje != 2)
                {
                    return acceso;
                }

                if (request.Items.Count == 0)
                {
                    return new Respuesta { IdTipoMensaje = 1, Mensaje = "Debe indicar al menos un documento a anular." };
                }

                const int idEmpresaFacturacion = 1; // TODO: resolver desde EMPRESAS de ms-facturación (GET /api/v1/empresas?idInquilino=) en vez de fijo.

                var tipos = await Task.WhenAll(request.Items.Select(async item =>
                {
                    var tipo = await _facturacionService.ObtenerTipoDocumentoAsync(usuarioLogueado.IdEmpresa, item.IdDocumentoElectronico, CancellationToken.None);
                    return (Item: item, TipoDocumentoCodigo: tipo?.Datos?.Cabecera?.TipoDocumentoCodigo, Referencia: tipo?.Datos?.Referencia);
                }));

                if (tipos.Any(t => t.TipoDocumentoCodigo is null))
                {
                    var idsFallidos = tipos.Where(t => t.TipoDocumentoCodigo is null).Select(t => t.Item.IdDocumentoElectronico);
                    return new Respuesta { IdTipoMensaje = 3, Mensaje = $"No se pudo resolver el tipo de uno o más documentos ({string.Join(",", idsFallidos)})." };
                }

                var itemsBoleta = tipos.Where(t => EsBoleta(t.TipoDocumentoCodigo, t.Referencia)).Select(t => t.Item).ToList();
                var itemsOtros = tipos.Where(t => !EsBoleta(t.TipoDocumentoCodigo, t.Referencia)).Select(t => t.Item).ToList();

                var comunicacionBajaTask = itemsOtros.Count > 0
                    ? _facturacionService.EnviarComunicacionBajaAsync(
                        new FacturacionComunicacionBajaRequest
                        {
                            IdInquilino = usuarioLogueado.IdEmpresa,
                            IdEmpresa = idEmpresaFacturacion,
                            FechaReferencia = request.FechaReferencia,
                            Items = itemsOtros.Select(item => new FacturacionItemBaja { IdDocumentoElectronico = item.IdDocumentoElectronico, MotivoDescripcion = item.MotivoDescripcion }).ToList()
                        },
                        CancellationToken.None)
                    : Task.FromResult<FacturacionEnvelope<FacturacionLoteDocumentoCreado>?>(null);

                var resumenBajaBoletaTask = itemsBoleta.Count > 0
                    ? _facturacionService.EnviarResumenBajaBoletaAsync(
                        new FacturacionComunicacionBajaRequest
                        {
                            IdInquilino = usuarioLogueado.IdEmpresa,
                            IdEmpresa = idEmpresaFacturacion,
                            FechaReferencia = request.FechaReferencia,
                            Items = itemsBoleta.Select(item => new FacturacionItemBaja { IdDocumentoElectronico = item.IdDocumentoElectronico, MotivoDescripcion = item.MotivoDescripcion }).ToList()
                        },
                        CancellationToken.None)
                    : Task.FromResult<FacturacionEnvelope<FacturacionLoteDocumentoCreado>?>(null);

                await Task.WhenAll(comunicacionBajaTask, resumenBajaBoletaTask);
                var comunicacionBajaResultado = await comunicacionBajaTask;
                var resumenBajaBoletaResultado = await resumenBajaBoletaTask;

                var lotesCreados = new List<FacturacionLoteDocumentoCreado>();
                var itemsExitosos = new List<AnularFacturaItem>();
                var mensajesError = new List<string>();
                // Factura y boleta usan códigos SUNAT distintos para "solicitud de baja enviada, pendiente
                // de respuesta" — ComunicacionBajaEnviada (6) vs. ResumenBajaEnviado (16) — así que cada
                // rama arma su propio tramo de documentosConEstado en vez de un IdEstadoFacturacion único.
                var documentosConEstado = new List<(int IdDocumentoElectronico, int IdEstadoFacturacion)>();

                if (itemsOtros.Count > 0)
                {
                    if (comunicacionBajaResultado is not null && comunicacionBajaResultado.IdTipoMensaje == 2 && comunicacionBajaResultado.Datos is not null)
                    {
                        lotesCreados.Add(comunicacionBajaResultado.Datos);
                        itemsExitosos.AddRange(itemsOtros);
                        documentosConEstado.AddRange(itemsOtros.Select(item => (item.IdDocumentoElectronico, IdEstadoFacturacion: 6))); // ComunicacionBajaEnviada
                    }
                    else
                    {
                        mensajesError.Add(comunicacionBajaResultado?.Mensaje ?? "No se pudo enviar la comunicación de baja.");
                    }
                }

                if (itemsBoleta.Count > 0)
                {
                    if (resumenBajaBoletaResultado is not null && resumenBajaBoletaResultado.IdTipoMensaje == 2 && resumenBajaBoletaResultado.Datos is not null)
                    {
                        lotesCreados.Add(resumenBajaBoletaResultado.Datos);
                        itemsExitosos.AddRange(itemsBoleta);
                        documentosConEstado.AddRange(itemsBoleta.Select(item => (item.IdDocumentoElectronico, IdEstadoFacturacion: 16))); // ResumenBajaEnviado
                    }
                    else
                    {
                        mensajesError.Add(resumenBajaBoletaResultado?.Mensaje ?? "No se pudo enviar el resumen de baja de boletas.");
                    }
                }

                if (documentosConEstado.Count > 0)
                {
                    var actualizacion = await _pedidoFacturaDao.ActualizarEstadoPorDocumentoAsync(usuarioLogueado.IdEmpresa, documentosConEstado);
                    if (actualizacion.IdTipoMensaje != 2)
                    {
                        _logger.LogWarning(
                            "No se pudo marcar la baja pendiente en los documentos {IdDocumentos} tras enviarla: {Mensaje}",
                            string.Join(",", documentosConEstado.Select(d => d.IdDocumentoElectronico)), actualizacion.Mensaje);
                    }
                }

                if (mensajesError.Count > 0)
                {
                    return new Respuesta { IdTipoMensaje = 1, Mensaje = string.Join(" ", mensajesError), Result = lotesCreados };
                }

                return new Respuesta { IdTipoMensaje = 2, Mensaje = "Comunicación/resumen de baja enviado correctamente.", Result = lotesCreados };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message };
            }
        }

        // Previsualiza AnularFacturasAsync sin ejecutar nada — mismas validaciones, y de poder enviarse la
        // lista de documentos que se verían incluidos (los indicados + las Notas vigentes arrastradas). Sin
        // MotivoDescripcion ni FechaReferencia (a diferencia de AnularFacturasAsync) — ninguno de los dos
        // hace falta para la validación.
        public async Task<Respuesta> PrevisualizarBajaAsync(UsuarioGeneral usuarioLogueado, List<int> idsDocumentoElectronico)
        {
            try
            {
                var acceso = await _pedidoFacturaDao.ValidarAccesoFacturacionAsync(usuarioLogueado, "previsualizar la comunicación de baja");
                if (acceso.IdTipoMensaje != 2)
                {
                    return acceso;
                }

                if (idsDocumentoElectronico.Count == 0)
                {
                    return new Respuesta { IdTipoMensaje = 1, Mensaje = "Debe indicar al menos un documento a anular." };
                }

                const int idEmpresaFacturacion = 1; // TODO: resolver desde EMPRESAS de ms-facturación (GET /api/v1/empresas?idInquilino=) en vez de fijo.

                // Mismo criterio de despacho por tipo que AnularFacturasAsync — ver ese método para el detalle.
                var tipos = await Task.WhenAll(idsDocumentoElectronico.Select(async id =>
                {
                    var tipo = await _facturacionService.ObtenerTipoDocumentoAsync(usuarioLogueado.IdEmpresa, id, CancellationToken.None);
                    return (Id: id, TipoDocumentoCodigo: tipo?.Datos?.Cabecera?.TipoDocumentoCodigo, Referencia: tipo?.Datos?.Referencia);
                }));

                if (tipos.Any(t => t.TipoDocumentoCodigo is null))
                {
                    var idsFallidos = tipos.Where(t => t.TipoDocumentoCodigo is null).Select(t => t.Id);
                    return new Respuesta { IdTipoMensaje = 3, Mensaje = $"No se pudo resolver el tipo de uno o más documentos ({string.Join(",", idsFallidos)})." };
                }

                var idsBoleta = tipos.Where(t => EsBoleta(t.TipoDocumentoCodigo, t.Referencia)).Select(t => t.Id).ToList();
                var idsOtros = tipos.Where(t => !EsBoleta(t.TipoDocumentoCodigo, t.Referencia)).Select(t => t.Id).ToList();

                var comunicacionBajaTask = idsOtros.Count > 0
                    ? _facturacionService.PrevisualizarBajaAsync(usuarioLogueado.IdEmpresa, idEmpresaFacturacion, idsOtros, CancellationToken.None)
                    : Task.FromResult<FacturacionEnvelope<List<FacturacionDocumentoBajaPreview>>?>(null);

                var resumenBajaBoletaTask = idsBoleta.Count > 0
                    ? _facturacionService.PrevisualizarResumenBajaBoletaAsync(usuarioLogueado.IdEmpresa, idEmpresaFacturacion, idsBoleta, CancellationToken.None)
                    : Task.FromResult<FacturacionEnvelope<List<FacturacionDocumentoBajaPreview>>?>(null);

                await Task.WhenAll(comunicacionBajaTask, resumenBajaBoletaTask);
                var comunicacionBajaResultado = await comunicacionBajaTask;
                var resumenBajaBoletaResultado = await resumenBajaBoletaTask;

                var mensajesError = new List<string>();
                if (idsOtros.Count > 0 && (comunicacionBajaResultado is null || comunicacionBajaResultado.IdTipoMensaje != 2))
                {
                    mensajesError.Add(comunicacionBajaResultado?.Mensaje ?? "No se pudo previsualizar la comunicación de baja.");
                }
                if (idsBoleta.Count > 0 && (resumenBajaBoletaResultado is null || resumenBajaBoletaResultado.IdTipoMensaje != 2))
                {
                    mensajesError.Add(resumenBajaBoletaResultado?.Mensaje ?? "No se pudo previsualizar el resumen de baja de boletas.");
                }

                if (mensajesError.Count > 0)
                {
                    return new Respuesta { IdTipoMensaje = 1, Mensaje = string.Join(" ", mensajesError) };
                }

                var preview = new List<FacturacionDocumentoBajaPreview>();
                if (comunicacionBajaResultado?.Datos is not null) preview.AddRange(comunicacionBajaResultado.Datos);
                if (resumenBajaBoletaResultado?.Datos is not null) preview.AddRange(resumenBajaBoletaResultado.Datos);

                return new Respuesta { IdTipoMensaje = 2, Mensaje = "Consulta exitosa.", Result = preview };
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

        // Rol validado acá (SP_PedidoFactura_ValidarAccesoResumen, maximilian_staging); el monto real viene
        // de ms-facturación (SP_DocumentoElectronico_ObtenerResumenFacturacion) — mismo patrón de dos pasos
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
