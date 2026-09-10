using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using SafetyReport.Application.Puertos.Pedido;
using SafetyReport.Application.Puertos.PedidoArchivo;
using SafetyReport.Application.Puertos.Almacenamiento;

namespace SafetyReport.Application.CasosDeUso
{
    public class PedidoArchivoHandler
    {
        private readonly IPedidoArchivoRepository _repository;
        private readonly IPedidoArchivoStorage _storage;
        private readonly FormatoDocumentoResolver _formatoDocumentoResolver;
        private readonly ILogger<PedidoArchivoHandler> _logger;

        public PedidoArchivoHandler(IPedidoArchivoRepository repository, IPedidoArchivoStorage storage, FormatoDocumentoResolver formatoDocumentoResolver, ILogger<PedidoArchivoHandler> logger)
        {
            _repository = repository;
            _storage = storage;
            _formatoDocumentoResolver = formatoDocumentoResolver;
            _logger = logger;
        }

        public async Task<Respuesta> CrearAsync(UsuarioGeneral usuarioLogueado, PedidoArchivoCrearBatch request)
        {
            try
            {
                var archivosPresignados = new List<PedidoArchivoPresignado>();
                Respuesta respuesta = new Respuesta();

                foreach (var archivo in request.Archivos)
                {
                    var formatoDocumento = await _formatoDocumentoResolver.ResolverAsync(usuarioLogueado, archivo.FormatoArchivo, archivo.NombreDocumento);
                    var rutaDefecto = _storage.GenerarRutaPedidoArchivo(request.IdPedido, archivo.NombreDocumento, 0);

                    var solicitudCrear = new PedidoArchivoCrear
                    {
                        IdPedido = request.IdPedido,
                        DocumentoURL = rutaDefecto,
                        NombreDocumento = archivo.NombreDocumento,
                        FormatoDocumento = formatoDocumento,
                        TamanoArchivo = archivo.TamanoArchivo,
                        IdTipoArchivo = archivo.IdTipoArchivo
                    };

                    var daoRespuesta = await _repository.CrearAsync(usuarioLogueado, solicitudCrear);
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

                    var urlSubida = _storage.GenerarUploadUrl(rutaArchivo, archivo.FormatoArchivo);

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
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<PedidoArchivoPresignado>()
                };
            }
        }

        public async Task<Respuesta> EditarAsync(UsuarioGeneral usuarioLogueado, PedidoArchivoEditar request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.FormatoDocumento))
                {
                    request.FormatoDocumento = await _formatoDocumentoResolver.ResolverAsync(usuarioLogueado, request.FormatoDocumento, request.NombreDocumento);
                }

                var respuestaObtener = await _repository.ObtenerAsync(usuarioLogueado, new PedidoArchivoIdRequest
                {
                    IdPedidoArchivo = request.IdPedidoArchivo,
                    IdPedido = request.IdPedido
                });

                if (respuestaObtener.IdTipoMensaje != 2)
                    return respuestaObtener;

                var existente = (respuestaObtener.Result as List<PedidoArchivoConsulta>)?.FirstOrDefault();
                if (existente is null)
                {
                    return new Respuesta
                    {
                        IdTipoMensaje = 1,
                        Mensaje = "No se encontró el archivo del pedido.",
                        Result = new List<PedidoArchivoConsulta>()
                    };
                }

                // Mantener TamanoArchivo existente si no se proporciona uno nuevo
                if (request.TamanoArchivo == 0)
                {
                    request.TamanoArchivo = existente.TamanoArchivo;
                }

                var rutaOrigen = existente.DocumentoURL;
                var rutaDestino = _storage.GenerarRutaPedidoArchivo(request.IdPedido, request.NombreDocumento, request.IdPedidoArchivo);

                // Solo mover S3 si cambia el nombre / ruta
                if (!string.Equals(rutaOrigen, rutaDestino, StringComparison.OrdinalIgnoreCase))
                {
                    await _storage.MoverArchivoAsync(rutaOrigen, rutaDestino);
                    request.DocumentoURL = rutaDestino;
                }
                else
                {
                    request.DocumentoURL = rutaOrigen;
                }

                var daoRespuesta = await _repository.EditarAsync(usuarioLogueado, request);

                return daoRespuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

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
                var daoRespuesta = await _repository.ObtenerAsync(usuarioLogueado, request);

                if (daoRespuesta.IdTipoMensaje == 2 && daoRespuesta.Result is List<PedidoArchivoConsulta> archivos)
                {
                    foreach (var archivo in archivos)
                    {
                        // Generar URL prefirmada para descarga (GET)
                        archivo.DownloadUrl = _storage.GenerarDownloadUrl(archivo.DocumentoURL);
                    }
                }

                return daoRespuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

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
                return await _repository.ListarAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

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
                var respuestaObtener = await _repository.ObtenerAsync(usuarioLogueado, request);

                if (respuestaObtener.IdTipoMensaje != 2)
                    return respuestaObtener;

                var existente = (respuestaObtener.Result as List<PedidoArchivoConsulta>)?.FirstOrDefault();
                if (existente is null)
                {
                    return new Respuesta
                    {
                        IdTipoMensaje = 1,
                        Mensaje = "No se encontró el archivo del pedido.",
                        Result = new List<PedidoArchivoEliminado>()
                    };
                }

                var daoRespuesta = await _repository.EliminarAsync(usuarioLogueado, request);

                if (daoRespuesta.IdTipoMensaje == 2)
                {
                    try
                    {
                        await _storage.DeleteFileAsync(existente.DocumentoURL);
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
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

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
