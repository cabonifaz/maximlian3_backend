using SafetyReport.DAO;
using SafetyReport.Models;

namespace SafetyReport.Handlers
{
    public class TablaMaestraHandler
    {
        private readonly TablaMaestraDAO _dao;
        private readonly BedrockTranslationService _translator;

        public TablaMaestraHandler(TablaMaestraDAO dao, BedrockTranslationService translator)
        {
            _dao = dao;
            _translator = translator;
        }

        public async Task<Respuesta> ListarAsync(UsuarioGeneral usuarioLogueado, int? idMaestro)
        {
            try
            {
                return await _dao.ListarAsync(usuarioLogueado, idMaestro);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = "Error interno del servidor.",
                    Result = new List<TablaMaestraItem>()
                };
            }
        }

        public async Task<Respuesta> ListarInventarioAsync(UsuarioGeneral usuarioLogueado, int? idMaestro)
        {
            try
            {
                return await _dao.ListarInventarioAsync(usuarioLogueado, idMaestro);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = "Error interno del servidor.",
                    Result = new List<InventarioMaestroItem>()
                };
            }
        }

        private static readonly HashSet<int> _maestrosSoloString1 = new() { 14, 44, 45, 47, 48, 49, 52, 56, 57, 58, 59, 60, 61 };

        public async Task<Respuesta> CrearAsync(UsuarioGeneral usuarioLogueado, TablaMaestraRequest request)
        {
            try
            {
                var respuesta = await _dao.CrearAsync(usuarioLogueado, request);

                if (respuesta.IdTipoMensaje == 2)
                {
                    var input = _maestrosSoloString1.Contains(request.IdMaestro)
                        ? new TranslationInput { String1 = request.String1 }
                        : new TranslationInput { String1 = request.String1, String2 = request.String2 };

                    var usuario = usuarioLogueado;
                    var req = request;

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var traduccion = await _translator.TranslateAsync(input);
                            await _dao.ActualizarTraduccionesAsync(usuario, req.IdMaestro, req.Num1, req.Num2, req.Num3, traduccion.String4, traduccion.String5, traduccion.String6, traduccion.String7);
                        }
                        catch (Exception) { }
                    });
                }

                return respuesta;
            }
            catch (Exception)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = "Error interno del servidor.",
                    Result = new List<TablaMaestraResultado>()
                };
            }
        }

        public async Task<Respuesta> EditarAsync(UsuarioGeneral usuarioLogueado, EditarTablaMaestraRequest request)
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
                    Mensaje = "Error interno del servidor.",
                    Result = new List<TablaMaestraResultado>()
                };
            }
        }

        public async Task<Respuesta> ObtenerAsync(UsuarioGeneral usuarioLogueado, ObtenerTablaMaestraRequest request)
        {
            try
            {
                return await _dao.ObtenerAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = "Error interno del servidor.",
                    Result = new List<TablaMaestraItem>()
                };
            }
        }

        public async Task<Respuesta> EliminarAsync(UsuarioGeneral usuarioLogueado, EliminarTablaMaestraRequest request)
        {
            try
            {
                return await _dao.EliminarAsync(usuarioLogueado, request.IdTablaMaestra);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = "Error interno del servidor.",
                    Result = new List<TablaMaestraResultado>()
                };
            }
        }
    }
}