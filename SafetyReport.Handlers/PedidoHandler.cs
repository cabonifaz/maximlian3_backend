using SafetyReport.DAO;
using SafetyReport.Models;

namespace SafetyReport.Handlers
{
    public class PedidoHandler
    {
        private readonly PedidoDAO _dao;
        private readonly PedidoArchivoDAO _pedidoArchivoDao;
        private readonly IS3UploadService _s3UploadService;

        public PedidoHandler(PedidoDAO dao, PedidoArchivoDAO pedidoArchivoDao, IS3UploadService s3UploadService)
        {
            _dao = dao;
            _pedidoArchivoDao = pedidoArchivoDao;
            _s3UploadService = s3UploadService;
        }

        public async Task<Respuesta> CrearAsync(UsuarioGeneral usuarioLogueado, Pedido request)
        {
            try
            {
                // Validaciones previas que exige el SP Pedido_INS:
                if (string.IsNullOrWhiteSpace(request.Codigo))
                    return new Respuesta { IdTipoMensaje = 1, Mensaje = "El código de pedido es obligatorio.", Result = new List<PedidoCreadoConArchivos>() };

                if (request.IdCliente <= 0)
                    return new Respuesta { IdTipoMensaje = 1, Mensaje = "IdCliente inválido.", Result = new List<PedidoCreadoConArchivos>() };

                if (request.IdTipoPersona <= 0)
                    return new Respuesta { IdTipoMensaje = 1, Mensaje = "IdTipoPersona inválido.", Result = new List<PedidoCreadoConArchivos>() };

                if (request.IdCompania <= 0)
                    return new Respuesta { IdTipoMensaje = 1, Mensaje = "IdCompania inválido.", Result = new List<PedidoCreadoConArchivos>() };

                if (string.IsNullOrWhiteSpace(request.InvestigarRazonSocialNombres))
                    request.InvestigarRazonSocialNombres = request.NombreCliente ?? string.Empty;

                if (request.IdTarifario <= 0)
                    return new Respuesta { IdTipoMensaje = 1, Mensaje = "IdTarifario inválido.", Result = new List<PedidoCreadoConArchivos>() };

                if (request.IdPlantilla <= 0)
                    return new Respuesta { IdTipoMensaje = 1, Mensaje = "IdPlantilla inválido.", Result = new List<PedidoCreadoConArchivos>() };

                if (request.IdIdioma <= 0)
                    return new Respuesta { IdTipoMensaje = 1, Mensaje = "IdIdioma inválido.", Result = new List<PedidoCreadoConArchivos>() };

                if (request.IdClaseInforme <= 0)
                    return new Respuesta { IdTipoMensaje = 1, Mensaje = "IdClaseInforme inválido.", Result = new List<PedidoCreadoConArchivos>() };

                if (request.IdEstado <= 0)
                    return new Respuesta { IdTipoMensaje = 1, Mensaje = "IdEstado inválido.", Result = new List<PedidoCreadoConArchivos>() };

                // Crea el pedido primero (sin archivos) para obtener el IdPedido real.
                var respuestaDao = await _dao.CrearAsync(usuarioLogueado, request, new List<(string RutaArchivo, string NombreDocumento, string FormatoDocumento, long TamanoArchivo )>());

                if (respuestaDao.IdTipoMensaje != 2)
                    return respuestaDao;

                var pedidos = respuestaDao.Result as List<PedidoCreado> ?? new List<PedidoCreado>();
                var idPedido = pedidos.FirstOrDefault()?.IdPedido ?? 0;

                var respuesta = new PedidoCreadoResponse { IdPedido = idPedido };

                if (idPedido > 0 && request.Archivos != null && request.Archivos.Count > 0)
                {
                    foreach (var archivo in request.Archivos)
                    {
                        var formatoDocumento = ResolverFormatoDocumento(archivo.TipoArchivo, archivo.NombreDocumento);
                        var rutaArchivo = _s3UploadService.GenerarRutaPedidoArchivo(idPedido, archivo.NombreDocumento);

                        var archivoCrear = new PedidoArchivoCrear
                        {
                            IdPedido = idPedido,
                            DocumentoURL = rutaArchivo,
                            NombreDocumento = archivo.NombreDocumento,
                            FormatoDocumento = formatoDocumento
                        };

                        var respuestaArchivo = await _pedidoArchivoDao.CrearAsync(usuarioLogueado, archivoCrear);

                        if (respuestaArchivo.IdTipoMensaje != 2)
                        {
                            return new Respuesta
                            {
                                IdTipoMensaje = respuestaArchivo.IdTipoMensaje,
                                Mensaje = respuestaArchivo.Mensaje,
                                Result = new List<PedidoCreadoResponse>()
                            };
                        }

                        respuesta.Archivos.Add(new PedidoArchivoPresignado
                        {
                            NombreDocumento = archivo.NombreDocumento,
                            RutaArchivo = rutaArchivo,
                            UploadUrl = _s3UploadService.GenerarUploadUrl(rutaArchivo, archivo.TipoArchivo)
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
                return await _dao.EditarAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<PedidoCreado>()
                };
            }
        }

        public async Task<Respuesta> ObtenerAsync(UsuarioGeneral usuarioLogueado, int idPedido)
        {
            try
            {
                return await _dao.ObtenerAsync(usuarioLogueado, idPedido);
            }
            catch (Exception ex)
            {
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
                return await _dao.ListarAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new PedidoListaResult()
                };
            }
        }

        public async Task<Respuesta> EliminarAsync(UsuarioGeneral usuarioLogueado, PedidoIdRequest request)
        {
            try
            {
                return await _dao.EliminarAsync(usuarioLogueado, request.IdPedido);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<PedidoEliminado>()
                };
            }
        }

        private static string ResolverFormatoDocumento(string tipoArchivo, string nombreArchivo)
        {
            var tipo = (tipoArchivo ?? string.Empty).Trim().ToUpperInvariant();

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
    }
}