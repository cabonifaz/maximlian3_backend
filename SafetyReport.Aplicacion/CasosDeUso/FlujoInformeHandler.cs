using Microsoft.Extensions.Logging;
using SafetyReport.Application.Puertos.FlujoInforme;

namespace SafetyReport.Application.CasosDeUso
{
    // SRC-F1-065: flujo secuencial analista → traductor, repositorio de informes y complementos.
    public class FlujoInformeHandler
    {
        private readonly IFlujoInformeRepository _flujoInformeRepository;
        private readonly ILogger<FlujoInformeHandler> _logger;

        public FlujoInformeHandler(IFlujoInformeRepository flujoInformeRepository, ILogger<FlujoInformeHandler> logger)
        {
            _flujoInformeRepository = flujoInformeRepository;
            _logger = logger;
        }

        public Task<Respuesta> ListarPorEtapaAsync(UsuarioGeneral usuarioLogueado, FiltroFlujoAsignacion filtro) =>
            EjecutarAsync(() => _flujoInformeRepository.ListarPorEtapaAsync(usuarioLogueado, filtro), new ListaFlujoAsignacion());

        public Task<Respuesta> AsignarRolAsync(UsuarioGeneral usuarioLogueado, AsignarRolFlujoRequest request)
        {
            var idRolAsignado = RolFlujoAsignacion.ObtenerIdRol(request.Rol);
            if (idRolAsignado is null)
            {
                return Task.FromResult(new Respuesta
                {
                    IdTipoMensaje = 1,
                    Mensaje = $"El rol debe ser '{RolFlujoAsignacion.Analista}' o '{RolFlujoAsignacion.Traductor}'.",
                    Result = new AsignacionFlujoCreada()
                });
            }

            return EjecutarAsync(
                () => _flujoInformeRepository.AsignarRolAsync(usuarioLogueado, request.IdPedido, idRolAsignado.Value, request.IdUsuarioAsignado),
                new AsignacionFlujoCreada());
        }

        public async Task<Respuesta> AsignarRolMasivoAsync(UsuarioGeneral usuarioLogueado, AsignarRolMasivoFlujoRequest request)
        {
            var resultado = new ResultadoAsignacionMasivaFlujo();
            var idsPedido = request.IdsPedido.Where(id => id > 0).Distinct().ToList();

            foreach (var idPedido in idsPedido)
            {
                var respuesta = await AsignarRolAsync(usuarioLogueado, new AsignarRolFlujoRequest
                {
                    IdPedido = idPedido,
                    Rol = request.Rol,
                    IdUsuarioAsignado = request.IdUsuarioAsignado
                });

                if (respuesta.IdTipoMensaje == 2)
                {
                    resultado.TotalAsignados++;
                    continue;
                }

                resultado.lstNoAsignado.Add(new PedidoNoAsignadoFlujo { IdPedido = idPedido, Mensaje = respuesta.Mensaje });
            }

            if (resultado.lstNoAsignado.Count == 0)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 2,
                    Mensaje = $"Se asignaron {resultado.TotalAsignados} pedido(s).",
                    Result = resultado
                };
            }

            var primerError = resultado.lstNoAsignado[0];
            return new Respuesta
            {
                IdTipoMensaje = 1,
                Mensaje = $"Se asignaron {resultado.TotalAsignados} de {idsPedido.Count} pedido(s). "
                    + $"Pedido {primerError.IdPedido}: {primerError.Mensaje}",
                Result = resultado
            };
        }

        public Task<Respuesta> ListarRepositorioAsync(UsuarioGeneral usuarioLogueado, FiltroRepositorioInformes filtro) =>
            EjecutarAsync(() => _flujoInformeRepository.ListarRepositorioAsync(usuarioLogueado, filtro), new ListaRepositorioInformes());

        public Task<Respuesta> ListarComplementosAsync(UsuarioGeneral usuarioLogueado, FiltroComplementos filtro) =>
            EjecutarAsync(() => _flujoInformeRepository.ListarComplementosAsync(usuarioLogueado, filtro), new ListaComplementos());

        public Task<Respuesta> CrearComplementoAsync(UsuarioGeneral usuarioLogueado, CrearComplementoRequest request) =>
            EjecutarAsync(() => _flujoInformeRepository.CrearComplementoAsync(usuarioLogueado, request), new ComplementoCreado());

        public Task<Respuesta> EnviarAprobacionComplementoAsync(UsuarioGeneral usuarioLogueado, EnviarAprobacionComplementoRequest request) =>
            EjecutarAsync(() => _flujoInformeRepository.EnviarAprobacionComplementoAsync(usuarioLogueado, request.IdComplemento), new ComplementoEnviado());

        public Task<Respuesta> ListarClientesDestinoComplementoAsync(UsuarioGeneral usuarioLogueado) =>
            EjecutarAsync(() => _flujoInformeRepository.ListarClientesDestinoComplementoAsync(usuarioLogueado), new ListaClientesDestinoComplemento());

        private async Task<Respuesta> EjecutarAsync(Func<Task<Respuesta>> accion, object resultadoVacio)
        {
            try
            {
                return await accion();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = resultadoVacio };
            }
        }
    }
}
