using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Extensions.Logging;
using SafetyReport.DAO;
using SafetyReport.Models;

namespace SafetyReport.Handlers
{
    public class CompaniaHandler
    {
        private readonly CompaniaDAO _dao;
        private readonly IS3UploadService _s3UploadService;
        private readonly FormatoDocumentoResolver _formatoDocumentoResolver;
        private readonly ILogger<CompaniaHandler> _logger;

        public CompaniaHandler(CompaniaDAO dao, IS3UploadService s3UploadService, FormatoDocumentoResolver formatoDocumentoResolver, ILogger<CompaniaHandler> logger)
        {
            _dao = dao;
            _s3UploadService = s3UploadService;
            _formatoDocumentoResolver = formatoDocumentoResolver;
            _logger = logger;
        }

        public async Task<Respuesta> CrearAsync(UsuarioGeneral usuarioLogueado, List<CompaniaCrear> lstCompanias)
        {
            try
            {
                return await _dao.CrearAsync(usuarioLogueado, lstCompanias);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new List<CompaniaCreada>() };
            }
        }

        public async Task<Respuesta> EditarAsync(UsuarioGeneral usuarioLogueado, CompaniaEditar request)
        {
            try
            {
                return await _dao.EditarAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new List<CompaniaCreada>() };
            }
        }

        public async Task<Respuesta> ObtenerAsync(UsuarioGeneral usuarioLogueado, CompaniaObtenerRequest request)
        {
            try
            {
                return await _dao.ObtenerAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new List<CompaniaConsulta>() };
            }
        }

        public async Task<Respuesta> ListarAsync(UsuarioGeneral usuarioLogueado, FiltroCompania filtro)
        {
            try
            {
                return await _dao.ListarAsync(usuarioLogueado, filtro);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new CompaniaListaResult() };
            }
        }

        public async Task<Respuesta> ListarMatchAsync(UsuarioGeneral usuarioLogueado, List<CompaniaMatchItem> lista)
        {
            try
            {
                return await _dao.ListarMatchAsync(usuarioLogueado, lista);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new List<CompaniaMatchResultItem>() };
            }
        }

        public async Task<Respuesta> EliminarAsync(UsuarioGeneral usuarioLogueado, int idCompania)
        {
            try
            {
                return await _dao.EliminarAsync(usuarioLogueado, idCompania);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new List<CompaniaEliminada>() };
            }
        }

        public async Task<Respuesta> CrearNoticiaAsync(UsuarioGeneral usuarioLogueado, CompaniaNoticiaCrear request)
        {
            try
            {
                await PrepararArchivosNoticiaAsync(usuarioLogueado, request.IdCompania, request.Archivos);
                var respuesta = await _dao.CrearNoticiaAsync(usuarioLogueado, request);
                AgregarArchivosPresignados(respuesta, request.Archivos);
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new List<CompaniaNoticiaCreada>() };
            }
        }

        public async Task<Respuesta> EditarNoticiaAsync(UsuarioGeneral usuarioLogueado, CompaniaNoticiaEditar request)
        {
            try
            {
                await PrepararArchivosNoticiaAsync(usuarioLogueado, request.IdCompania, request.Archivos);
                var respuesta = await _dao.EditarNoticiaAsync(usuarioLogueado, request);
                AgregarArchivosPresignados(respuesta, request.Archivos);
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new List<CompaniaNoticiaCreada>() };
            }
        }

        public async Task<Respuesta> ObtenerNoticiaAsync(UsuarioGeneral usuarioLogueado, CompaniaNoticiaObtenerRequest request)
        {
            try
            {
                return await _dao.ObtenerNoticiaAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new List<CompaniaNoticiaConsulta>() };
            }
        }

        public async Task<Respuesta> ObtenerNoticiaArchivoAsync(UsuarioGeneral usuarioLogueado, CompaniaNoticiaArchivoIdRequest request)
        {
            try
            {
                var respuesta = await _dao.ObtenerNoticiaArchivoAsync(usuarioLogueado, request.IdCompaniaNoticiaArchivo);
                if (respuesta.IdTipoMensaje == 2 && respuesta.Result is List<CompaniaNoticiaArchivoDescargaConsulta> archivos && archivos.Count > 0)
                {
                    var archivo = archivos[0];
                    if (!string.IsNullOrWhiteSpace(archivo.ArchivoUrl))
                        archivo.DownloadUrl = _s3UploadService.GenerarDownloadUrl(archivo.ArchivoUrl, archivo.NombreDocumento ?? archivo.ArchivoUrl);
                    archivo.ArchivoUrl = string.Empty;
                }
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new List<CompaniaNoticiaArchivoDescargaConsulta>() };
            }
        }

        public async Task<Respuesta> EliminarNoticiaArchivoAsync(UsuarioGeneral usuarioLogueado, CompaniaNoticiaArchivoIdRequest request)
        {
            try
            {
                var respuesta = await _dao.EliminarNoticiaArchivoAsync(usuarioLogueado, request.IdCompaniaNoticiaArchivo);
                if (respuesta.IdTipoMensaje == 2 && respuesta.Result is List<CompaniaNoticiaArchivoEliminado> archivos && archivos.Count > 0)
                {
                    var archivo = archivos[0];
                    if (!string.IsNullOrWhiteSpace(archivo.ArchivoUrl))
                    {
                        try
                        {
                            await _s3UploadService.DeleteFileAsync(archivo.ArchivoUrl);
                        }
                        catch
                        {
                        }
                    }
                    archivo.ArchivoUrl = string.Empty;
                }
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new List<CompaniaNoticiaArchivoEliminado>() };
            }
        }

        public async Task<Respuesta> ListarNoticiasAsync(UsuarioGeneral usuarioLogueado, FiltroCompaniaNoticia filtro)
        {
            try
            {
                return await _dao.ListarNoticiasAsync(usuarioLogueado, filtro);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new CompaniaNoticiaListaResult() };
            }
        }

        public async Task<Respuesta> EliminarNoticiaAsync(UsuarioGeneral usuarioLogueado, int idCompaniaNoticia)
        {
            try
            {
                var obtener = await _dao.ObtenerNoticiaAsync(usuarioLogueado, new CompaniaNoticiaObtenerRequest
                {
                    IdCompaniaNoticia = idCompaniaNoticia
                });

                var respuesta = await _dao.EliminarNoticiaAsync(usuarioLogueado, idCompaniaNoticia);

                if (respuesta.IdTipoMensaje == 2)
                    await EliminarArchivosS3Async(obtener);

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new List<CompaniaNoticiaEliminada>() };
            }
        }

        public async Task<Respuesta> ListarNoticiasBalanceAsync(UsuarioGeneral usuarioLogueado, FiltroCompaniaNoticiaBalance filtro)
        {
            try
            {
                return await _dao.ListarNoticiasBalanceAsync(usuarioLogueado, filtro);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new CompaniaNoticiaBalanceListaResult() };
            }
        }

        public async Task<Respuesta> ObtenerNoticiaBalanceAsync(UsuarioGeneral usuarioLogueado, CompaniaNoticiaBalanceObtenerRequest request)
        {
            try
            {
                return await _dao.ObtenerNoticiaBalanceAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new List<CompaniaNoticiaBalanceConsulta>() };
            }
        }

        public async Task<Respuesta> ListarNoticiasDetalleAsync(UsuarioGeneral usuarioLogueado, FiltroCompaniaNoticiaDetalle filtro)
        {
            try
            {
                return await _dao.ListarNoticiasDetalleAsync(usuarioLogueado, filtro);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new CompaniaNoticiaDetalleListaResult() };
            }
        }

        public async Task<Respuesta> ExportarNoticiasDetalleAsync(UsuarioGeneral usuarioLogueado, FiltroCompaniaNoticiaDetalle filtro)
        {
            try
            {
                var respuesta = await _dao.ExportarNoticiasDetalleAsync(usuarioLogueado, filtro);
                if (respuesta.IdTipoMensaje != 2)
                    return respuesta;

                var items = respuesta.Result as List<CompaniaNoticiaDetalleListaConsulta> ?? new();
                var archivo = GenerarExcelNoticiasDetalle(items);

                respuesta.Result = new CompaniaNoticiaDetalleExportacion
                {
                    NombreArchivo = $"companias-noticia-detalle-{DateTime.Now:yyyyMMddHHmmss}.xlsx",
                    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    Archivo = archivo
                };
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = null! };
            }
        }

        private async Task PrepararArchivosNoticiaAsync(UsuarioGeneral usuarioLogueado, int idCompania, List<CompaniaNoticiaArchivoItem>? archivos)
        {
            if (archivos == null)
                return;

            foreach (var archivo in archivos)
            {
                if (string.IsNullOrWhiteSpace(archivo.NombreArchivo) && string.IsNullOrWhiteSpace(archivo.ArchivoUrl))
                    continue;

                var rutaArchivo = string.IsNullOrWhiteSpace(archivo.NombreArchivo)
                    ? archivo.ArchivoUrl!
                    : GenerarRutaCompaniaNoticiaArchivo(idCompania, archivo.NombreArchivo);
                var formatoArchivo = string.IsNullOrWhiteSpace(archivo.FormatoArchivo)
                    ? "application/octet-stream"
                    : archivo.FormatoArchivo;
                var nombreDocumento = string.IsNullOrWhiteSpace(archivo.NombreDocumento)
                    ? archivo.NombreArchivo
                    : archivo.NombreDocumento;

                archivo.ArchivoUrl = rutaArchivo;
                archivo.NombreDocumento = nombreDocumento;
                archivo.Extension = await _formatoDocumentoResolver.ResolverAsync(
                    usuarioLogueado,
                    archivo.FormatoArchivo,
                    nombreDocumento ?? archivo.ArchivoUrl);
                archivo.UploadUrl = _s3UploadService.GenerarUploadUrl(rutaArchivo, formatoArchivo);
            }
        }

        private void AgregarArchivosPresignados(Respuesta respuesta, List<CompaniaNoticiaArchivoItem>? archivos)
        {
            if (archivos == null || respuesta.IdTipoMensaje != 2 || respuesta.Result is not List<CompaniaNoticiaCreada> noticias || noticias.Count == 0)
                return;

            noticias[0].Archivos = archivos
                .Where(a => !string.IsNullOrWhiteSpace(a.UploadUrl))
                .ToList();
        }

        // TODO: EliminarNoticiaAsync no longer deletes S3 files here — CompaniaNoticia_Obtener
        // stopped returning ArchivoUrl. Pending replacement of the delete flow.
        private async Task EliminarArchivosS3Async(Respuesta obtener)
        {
            await Task.CompletedTask;
        }

        private static string GenerarRutaCompaniaNoticiaArchivo(int idCompania, string nombreArchivo)
        {
            var extension = Path.GetExtension(nombreArchivo);
            var nombreBase = Path.GetFileNameWithoutExtension(nombreArchivo);
            var nombreLimpio = string.Concat(nombreBase.Where(c => char.IsLetterOrDigit(c) || c == '_' || c == '-'));

            if (string.IsNullOrWhiteSpace(nombreLimpio))
                nombreLimpio = "archivo";

            return $"companias/{idCompania}/noticias/adjuntos/{nombreLimpio}-{Guid.NewGuid():N}{extension}";
        }

        private static byte[] GenerarExcelNoticiasDetalle(List<CompaniaNoticiaDetalleListaConsulta> items)
        {
            using var stream = new MemoryStream();
            using (var document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
            {
                var workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                var sheetData = new SheetData();
                worksheetPart.Worksheet = new Worksheet();

                worksheetPart.Worksheet.Append(CrearColumnasExcel());
                worksheetPart.Worksheet.Append(sheetData);

                var sheets = workbookPart.Workbook.AppendChild(new Sheets());
                sheets.Append(new Sheet
                {
                    Id = workbookPart.GetIdOfPart(worksheetPart),
                    SheetId = 1,
                    Name = "Detalle"
                });

                sheetData.Append(CrearFilaExcel(
                    "IdCompania",
                    "Nombre Completo",
                    "Numero Documento",
                    "Pais",
                    "Bandera",
                    "Direccion",
                    "Telefono",
                    "Actividad Comercial",
                    "Numero Empleados"));

                foreach (var item in items)
                {
                    sheetData.Append(CrearFilaExcel(
                        item.IdCompania.ToString(),
                        item.NombreCompleto,
                        item.NumeroDocumento,
                        item.Pais,
                        item.Bandera,
                        item.Direccion,
                        item.Telefono,
                        item.ActividadComercial,
                        item.NumeroEmpleados?.ToString()));
                }

                workbookPart.Workbook.Save();
            }

            return stream.ToArray();
        }

        private static Columns CrearColumnasExcel()
        {
            return new Columns(
                CrearColumnaExcel(1, 1, 12),
                CrearColumnaExcel(2, 2, 35),
                CrearColumnaExcel(3, 3, 22),
                CrearColumnaExcel(4, 4, 20),
                CrearColumnaExcel(5, 5, 14),
                CrearColumnaExcel(6, 6, 45),
                CrearColumnaExcel(7, 7, 18),
                CrearColumnaExcel(8, 8, 30),
                CrearColumnaExcel(9, 9, 18));
        }

        private static Column CrearColumnaExcel(uint min, uint max, double width)
        {
            return new Column
            {
                Min = min,
                Max = max,
                Width = width,
                CustomWidth = true
            };
        }

        private static Row CrearFilaExcel(params string?[] valores)
        {
            var row = new Row();
            foreach (var valor in valores)
                row.Append(CrearCeldaExcel(valor));
            return row;
        }

        private static Cell CrearCeldaExcel(string? valor)
        {
            return new Cell
            {
                DataType = CellValues.InlineString,
                InlineString = new InlineString(new Text(valor ?? string.Empty))
            };
        }
    }
}
