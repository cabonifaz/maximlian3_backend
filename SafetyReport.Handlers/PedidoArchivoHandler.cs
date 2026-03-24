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

                foreach (var archivo in request.Archivos)
                {

                    var formatoDocumento = ResolverFormatoDocumento(archivo.FormatoArchivo, archivo.NombreDocumento);
                    var rutaArchivo = _s3UploadService.GenerarRutaPedidoArchivo(request.IdPedido, archivo.NombreDocumento);

                    var crearRequest = new PedidoArchivoCrear
                    {
                        IdPedido = request.IdPedido,
                        DocumentoURL = rutaArchivo,
                        NombreDocumento = archivo.NombreDocumento,
                        FormatoDocumento = formatoDocumento,
                        TamanoArchivo = archivo.TamanoArchivo,
                        IdTipoArchivo = archivo.IdTipoArchivo
                    };

                    var daoResponse = await _dao.CrearAsync(usuarioLogueado, crearRequest);
                    if (daoResponse.IdTipoMensaje != 2)
                    {
                        return new Respuesta
                        {
                            IdTipoMensaje = daoResponse.IdTipoMensaje,
                            Mensaje = daoResponse.Mensaje,
                            Result = new List<PedidoArchivoPresignado>()
                        };
                    }

                    var uploadUrl = _s3UploadService.GenerarUploadUrl(rutaArchivo, archivo.FormatoArchivo);

                    archivosPresignados.Add(new PedidoArchivoPresignado
                    {
                        NombreDocumento = archivo.NombreDocumento,
                        RutaArchivo = rutaArchivo,
                        UploadUrl = uploadUrl
                    });
                }

                return new Respuesta
                {
                    IdTipoMensaje = 2,
                    Mensaje = "Archivos insertados correctamente",
                    Result = archivosPresignados
                };
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 1,
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
                _ => Path.GetExtension(nombreArchivo).TrimStart('.').ToUpperInvariant()
            };
        }

        public async Task<Respuesta> SubirAsync(UsuarioGeneral usuarioLogueado, PedidoArchivoSubirRequest request)
        {
            try
            {
                if (request.Archivo == null || request.Archivo.Length == 0)
                {
                    return new Respuesta
                    {
                        IdTipoMensaje = 1,
                        Mensaje = "Archivo inválido",
                        Result = new List<PedidoArchivoSubido>()
                    };
                }

                var rutaArchivo = _s3UploadService.GenerarRutaPedidoArchivo(request.IdPedido, request.NombreArchivo);

                await _s3UploadService.UploadFileAsync(rutaArchivo, request.Archivo);

                var crearRequest = new PedidoArchivoCrear
                {
                    IdPedido = request.IdPedido,
                    DocumentoURL = rutaArchivo,
                    NombreDocumento = request.NombreArchivo,
                    FormatoDocumento = request.TipoArchivo
                };

                var daoResponse = await _dao.CrearAsync(usuarioLogueado, crearRequest);

                if (daoResponse.IdTipoMensaje != 2)
                    return daoResponse;

                var idCreado = (daoResponse.Result as List<PedidoArchivoCreado>)?.FirstOrDefault()?.IdPedidoArchivo ?? 0;

                return new Respuesta
                {
                    IdTipoMensaje = 2,
                    Mensaje = daoResponse.Mensaje,
                    Result = new List<PedidoArchivoSubido>
                    {
                        new PedidoArchivoSubido { IdPedidoArchivo = idCreado, RutaArchivo = rutaArchivo }
                    }
                };
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 1,
                    Mensaje = ex.Message,
                    Result = new List<PedidoArchivoSubido>()
                };
            }
        }

        public async Task<Respuesta> EditarAsync(UsuarioGeneral usuarioLogueado, PedidoArchivoEditar request)
        {
            try
            {
                if (request.IdPedidoArchivo <= 0)
                {
                    return new Respuesta
                    {
                        IdTipoMensaje = 1,
                        Mensaje = "IdPedidoArchivo inválido",
                        Result = new List<PedidoArchivoCreado>()
                    };
                }

                if (string.IsNullOrWhiteSpace(request.NombreDocumento))
                {
                    return new Respuesta
                    {
                        IdTipoMensaje = 1,
                        Mensaje = "NombreDocumento es requerido",
                        Result = new List<PedidoArchivoCreado>()
                    };
                }

                if (string.IsNullOrWhiteSpace(request.FormatoDocumento))
                {
                    request.FormatoDocumento = ResolverFormatoDocumento(request.FormatoDocumento, request.NombreDocumento);
                }

                var obtenerResponse = await _dao.ObtenerAsync(usuarioLogueado, new PedidoArchivoIdRequest
                {
                    IdPedidoArchivo = request.IdPedidoArchivo,
                    IdPedido = request.IdPedido
                });

                if (obtenerResponse.IdTipoMensaje != 2)
                    return obtenerResponse;

                var existente = (obtenerResponse.Result as List<PedidoArchivoConsulta>)?.FirstOrDefault();

                if (existente == null)
                {
                    return new Respuesta
                    {
                        IdTipoMensaje = 1,
                        Mensaje = "No se encontró el archivo solicitado",
                        Result = new List<PedidoArchivoCreado>()
                    };
                }

                // Mantener TamanoArchivo existente si no se proporciona uno nuevo
                if (request.TamanoArchivo == 0)
                {
                    request.TamanoArchivo = existente.TamanoArchivo;
                }

                var rutaOrigen = existente.DocumentoURL;
                var rutaDestino = _s3UploadService.GenerarRutaPedidoArchivo(request.IdPedido, request.NombreDocumento);

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

                var daoResponse = await _dao.EditarAsync(usuarioLogueado, request);

                return daoResponse;
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 1,
                    Mensaje = ex.Message,
                    Result = new List<PedidoArchivoCreado>()
                };
            }
        }

        public async Task<Respuesta> ObtenerAsync(UsuarioGeneral usuarioLogueado, PedidoArchivoIdRequest request)
        {
            try
            {
                var daoResponse = await _dao.ObtenerAsync(usuarioLogueado, request);

                if (daoResponse.IdTipoMensaje == 2 && daoResponse.Result is List<PedidoArchivoConsulta> archivos)
                {
                    foreach (var archivo in archivos)
                    {
                        // Generar URL prefirmada para descarga (GET)
                        archivo.DownloadUrl = _s3UploadService.GenerarDownloadUrl(archivo.DocumentoURL);
                    }
                }

                return daoResponse;
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 1,
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
                    IdTipoMensaje = 1,
                    Mensaje = ex.Message,
                    Result = new PedidoArchivoListaResult()
                };
            }
        }

        public async Task<Respuesta> EliminarAsync(UsuarioGeneral usuarioLogueado, PedidoArchivoIdRequest request)
        {
            try
            {
                var obtenerResponse = await _dao.ObtenerAsync(usuarioLogueado, request);

                if (obtenerResponse.IdTipoMensaje != 2)
                    return obtenerResponse;

                var existente = (obtenerResponse.Result as List<PedidoArchivoConsulta>)?.FirstOrDefault();
                if (existente == null)
                {
                    return new Respuesta
                    {
                        IdTipoMensaje = 1,
                        Mensaje = "No se encontró el archivo a eliminar",
                        Result = new List<PedidoArchivoEliminado>()
                    };
                }

                var daoResponse = await _dao.EliminarAsync(usuarioLogueado, request);

                if (daoResponse.IdTipoMensaje == 2)
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

                return daoResponse;
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 1,
                    Mensaje = ex.Message,
                    Result = new List<PedidoArchivoEliminado>()
                };
            }
        }
    }
}