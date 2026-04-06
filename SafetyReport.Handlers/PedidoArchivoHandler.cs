using System.IO;
using System.Linq;
using SafetyReport.DAO;
using SafetyReport.Models;

namespace SafetyReport.Handlers
{
    public class PedidoArchivoHandler
    {
        private readonly PedidoArchivoDAO _dao;
        private readonly IS3UploadService _s3UploadService;

        public PedidoArchivoHandler(PedidoArchivoDAO dao, IS3UploadService s3UploadService)
        {
            _dao = dao;
            _s3UploadService = s3UploadService;
        }

        public async Task<Respuesta> CrearAsync(UsuarioGeneral usuarioLogueado, PedidoArchivoCrearBatch request)
        {
            try
            {
                var archivosPresignados = new List<PedidoArchivoPresignado>();
                Respuesta respuesta = new Respuesta();

                foreach (var archivo in request.Archivos)
                {
                    var formatoDocumento = ResolverFormatoDocumento(archivo.FormatoArchivo, archivo.NombreDocumento);
                    var rutaDefecto = _s3UploadService.GenerarRutaPedidoArchivo(request.IdPedido, archivo.NombreDocumento, 0);

                    var solicitudCrear = new PedidoArchivoCrear
                    {
                        IdPedido = request.IdPedido,
                        DocumentoURL = rutaDefecto,
                        NombreDocumento = archivo.NombreDocumento,
                        FormatoDocumento = formatoDocumento,
                        TamanoArchivo = archivo.TamanoArchivo,
                        IdTipoArchivo = archivo.IdTipoArchivo
                    };

                    var daoRespuesta = await _dao.CrearAsync(usuarioLogueado, solicitudCrear);
                    if (daoRespuesta.IdTipoMensaje != 2)
                    {
                        return new Respuesta
                        {
                            IdTipoMensaje = daoRespuesta.IdTipoMensaje,
                            Mensaje = daoRespuesta.Mensaje,
                            Result = new List<PedidoArchivoPresignado>()
                        };
                    }

                    respuesta = daoRespuesta;

                    var archivosCreados = daoRespuesta.Result as List<PedidoArchivoCreado> ?? [];
                    var rutaArchivo = archivosCreados.FirstOrDefault()?.DocumentoURL ?? rutaDefecto;

                    var urlSubida = _s3UploadService.GenerarUploadUrl(rutaArchivo, archivo.FormatoArchivo);

                    archivosPresignados.Add(new PedidoArchivoPresignado
                    {
                        NombreDocumento = archivo.NombreDocumento,
                        RutaArchivo = rutaArchivo,
                        UploadUrl = urlSubida
                    });
                }

                return new Respuesta
                {
                    IdTipoMensaje = respuesta.IdTipoMensaje,
                    Mensaje = respuesta.Mensaje,
                    Result = archivosPresignados
                };
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<PedidoArchivoPresignado>()
                };
            }
        }

        private static string ResolverFormatoDocumento(string formatoArchivo, string nombreArchivo)
        {
            var tipo = (formatoArchivo ?? string.Empty).Trim().ToUpperInvariant();

            return tipo switch
            {
                "IMAGE/JPEG" => "JPG",
                "IMAGE/JPG" => "JPG",
                "IMAGE/PNG" => "PNG",
                "APPLICATION/PDF" => "PDF",
                "APPLICATION/MSWORD" => "DOC",
                "APPLICATION/VND.OPENXMLFORMATS-OFFICEDOCUMENT.WORDPROCESSINGML.DOCUMENT" => "DOCX",
                "APPLICATION/VND.MS-EXCEL" => "XLS",
                "APPLICATION/VND.OPENXMLFORMATS-OFFICEDOCUMENT.SPREADSHEETML.SHEET" => "XLSX",
                "TEXT/PLAIN" => "TXT",
                "APPLICATION/OCTET-STREAM" => "TXT",
                "TEXT/HTML" => "HTML",
                _ => Path.GetExtension(nombreArchivo).TrimStart('.').ToUpperInvariant()
            };
        }

        public async Task<Respuesta> EditarAsync(UsuarioGeneral usuarioLogueado, PedidoArchivoEditar request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.FormatoDocumento))
                {
                    request.FormatoDocumento = ResolverFormatoDocumento(request.FormatoDocumento, request.NombreDocumento);
                }

                var respuestaObtener = await _dao.ObtenerAsync(usuarioLogueado, new PedidoArchivoIdRequest
                {
                    IdPedidoArchivo = request.IdPedidoArchivo,
                    IdPedido = request.IdPedido
                });

                if (respuestaObtener.IdTipoMensaje != 2)
                    return respuestaObtener;

                var existente = (respuestaObtener.Result as List<PedidoArchivoConsulta>)?.FirstOrDefault();

                // Mantener TamanoArchivo existente si no se proporciona uno nuevo
                if (request.TamanoArchivo == 0)
                {
                    request.TamanoArchivo = existente.TamanoArchivo;
                }

                var rutaOrigen = existente.DocumentoURL;
                var rutaDestino = _s3UploadService.GenerarRutaPedidoArchivo(request.IdPedido, request.NombreDocumento, request.IdPedidoArchivo);

                // Solo mover S3 si cambia el nombre / ruta
                if (!string.Equals(rutaOrigen, rutaDestino, StringComparison.OrdinalIgnoreCase))
                {
                    await _s3UploadService.MoverArchivoAsync(rutaOrigen, rutaDestino);
                    request.DocumentoURL = rutaDestino;
                }
                else
                {
                    request.DocumentoURL = rutaOrigen;
                }

                var daoRespuesta = await _dao.EditarAsync(usuarioLogueado, request);

                return daoRespuesta;
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<PedidoArchivoCreado>()
                };
            }
        }

        public async Task<Respuesta> ObtenerAsync(UsuarioGeneral usuarioLogueado, PedidoArchivoIdRequest request)
        {
            try
            {
                var daoRespuesta = await _dao.ObtenerAsync(usuarioLogueado, request);

                if (daoRespuesta.IdTipoMensaje == 2 && daoRespuesta.Result is List<PedidoArchivoConsulta> archivos)
                {
                    foreach (var archivo in archivos)
                    {
                        // Generar URL prefirmada para descarga (GET)
                        archivo.DownloadUrl = _s3UploadService.GenerarDownloadUrl(archivo.DocumentoURL);
                    }
                }

                return daoRespuesta;
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<PedidoArchivoConsulta>()
                };
            }
        }

        public async Task<Respuesta> ListarAsync(UsuarioGeneral usuarioLogueado, FiltroPedidoArchivo request)
        {
            try
            {
                return await _dao.ListarAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new PedidoArchivoListaResult()
                };
            }
        }

        public async Task<Respuesta> EliminarAsync(UsuarioGeneral usuarioLogueado, PedidoArchivoIdRequest request)
        {
            try
            {
                var respuestaObtener = await _dao.ObtenerAsync(usuarioLogueado, request);

                if (respuestaObtener.IdTipoMensaje != 2)
                    return respuestaObtener;

                var existente = (respuestaObtener.Result as List<PedidoArchivoConsulta>)?.FirstOrDefault();

                var daoRespuesta = await _dao.EliminarAsync(usuarioLogueado, request);

                if (daoRespuesta.IdTipoMensaje == 2)
                {
                    try
                    {
                        await _s3UploadService.DeleteFileAsync(existente.DocumentoURL);
                    }
                    catch
                    {
                        // No hacemos rollback si falla S3, la eliminación en BD ya fue realizada.
                    }
                }

                return daoRespuesta;
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<PedidoArchivoEliminado>()
                };
            }
        }
    }
}