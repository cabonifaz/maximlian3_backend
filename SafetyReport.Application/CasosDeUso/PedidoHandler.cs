using Microsoft.Extensions.Logging;
using SafetyReport.Application.Puertos.Pedido;
using SafetyReport.Application.Puertos.PedidoArchivo;
using SafetyReport.Application.Puertos.Almacenamiento;
using SafetyReport.Models;

namespace SafetyReport.Application.CasosDeUso
{
    public class PedidoHandler
    {
        private readonly IPedidoRepository _repository;
        private readonly IPedidoArchivoRepository _pedidoArchivoRepository;
        private readonly IPedidoArchivoStorage _pedidoArchivoStorage;
        private readonly FormatoDocumentoResolver _formatoDocumentoResolver;
        private readonly ILogger<PedidoHandler> _logger;

        public PedidoHandler(IPedidoRepository repository, IPedidoArchivoRepository pedidoArchivoRepository, IPedidoArchivoStorage pedidoArchivoStorage, FormatoDocumentoResolver formatoDocumentoResolver, ILogger<PedidoHandler> logger)
        {
            _repository = repository;
            _pedidoArchivoRepository = pedidoArchivoRepository;
            _pedidoArchivoStorage = pedidoArchivoStorage;
            _formatoDocumentoResolver = formatoDocumentoResolver;
            _logger = logger;
        }

        public async Task<Respuesta> CrearAsync(UsuarioGeneral usuarioLogueado, Pedido request)
        {
            try
            {
                // Crea el pedido primero (sin archivos) para obtener el IdPedido real.
                var respuestaDao = await _repository.CrearAsync(usuarioLogueado, request);

                if (respuestaDao.IdTipoMensaje != 2)
                    return respuestaDao;

                var pedidos = respuestaDao.Result as List<PedidoCreado> ?? new List<PedidoCreado>();
                var idPedido = pedidos.FirstOrDefault()?.IdPedido ?? 0;

                var respuesta = new PedidoCreadoResponse { IdPedido = idPedido };

                if (idPedido > 0 && request.Archivos != null && request.Archivos.Count > 0)
                {
                    foreach (var archivo in request.Archivos)
                    {
                        var formatoDocumento = await _formatoDocumentoResolver.ResolverAsync(usuarioLogueado, archivo.FormatoArchivo, archivo.NombreDocumento);
                        var rutaDefecto = _pedidoArchivoStorage.GenerarRutaPedidoArchivo(idPedido, archivo.NombreDocumento, 0);

                        var archivoCrear = new PedidoArchivoCrear
                        {
                            IdPedido = idPedido,
                            DocumentoURL = rutaDefecto,
                            NombreDocumento = archivo.NombreDocumento,
                            FormatoDocumento = formatoDocumento,
                            TamanoArchivo = archivo.TamanoArchivo,
                            IdTipoArchivo = archivo.IdTipoArchivo
                        };

                        var respuestaArchivo = await _pedidoArchivoRepository.CrearAsync(usuarioLogueado, archivoCrear);

                        if (respuestaArchivo.IdTipoMensaje != 2)
                        {
                            return new Respuesta
                            {
                                IdTipoMensaje = respuestaArchivo.IdTipoMensaje,
                                Mensaje = respuestaArchivo.Mensaje,
                                Result = new List<PedidoCreadoResponse>()
                            };
                        }

                        var archivosCreados = respuestaArchivo.Result as List<PedidoArchivoCreado> ?? [];
                        var rutaArchivo = archivosCreados.FirstOrDefault()?.DocumentoURL ?? rutaDefecto;

                        respuesta.Archivos.Add(new PedidoArchivoPresignado
                        {
                            NombreDocumento = archivo.NombreDocumento,
                            RutaArchivo = rutaArchivo,
                            UploadUrl = _pedidoArchivoStorage.GenerarUploadUrl(rutaArchivo, archivo.FormatoArchivo)
                        });
                    }
                }

                return new Respuesta
                {
                    IdTipoMensaje = 2,
                    Mensaje = respuestaDao.Mensaje,
                    Result = new List<PedidoCreadoResponse> { respuesta }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<PedidoCreadoConArchivos>()
                };
            }
        }

        public async Task<Respuesta> EditarAsync(UsuarioGeneral usuarioLogueado, EditarPedido request)
        {
            try
            {
                return await _repository.EditarAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<PedidoCreado>()
                };
            }
        }

        public async Task<Respuesta> ObtenerAsync(UsuarioGeneral usuarioLogueado, FiltroPedidoObtener request)
        {
            try
            {
                return await _repository.ObtenerAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<PedidoConsulta>()
                };
            }
        }

        public async Task<Respuesta> ListarAsync(UsuarioGeneral usuarioLogueado, FiltroPedido request)
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
                    Result = new PedidoListaResult()
                };
            }
        }

        public async Task<Respuesta> ListarAsignacionAsync(UsuarioGeneral usuarioLogueado, FiltroPedidoAsignacion request)
        {
            try
            {
                return await _repository.ListarAsignacionAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new PedidoAsignacionListaResult()
                };
            }
        }

        public async Task<Respuesta> CancelarAsync(UsuarioGeneral usuarioLogueado, PedidoIdRequest request)
        {
            try
            {
                return await _repository.CancelarAsync(usuarioLogueado, request.IdPedido);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<PedidoEliminado>()
                };
            }
        }

        public async Task<Respuesta> EliminarAsync(UsuarioGeneral usuarioLogueado, PedidoIdRequest request)
        {
            try
            {
                return await _repository.EliminarAsync(usuarioLogueado, request.IdPedido);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<PedidoEliminado>()
                };
            }
        }

        public async Task<Respuesta> ObtenerResumenAsync(UsuarioGeneral usuarioLogueado)
        {
            try
            {
                return await _repository.ObtenerResumenAsync(usuarioLogueado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<PedidoEstadoResumenItem>()
                };
            }
        }

    }
}
