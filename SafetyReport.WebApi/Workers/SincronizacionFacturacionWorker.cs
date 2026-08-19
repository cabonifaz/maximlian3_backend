using SafetyReport.DAO;
using SafetyReport.Handlers;
using SafetyReport.Models;

namespace SafetyReport.WebApi.Workers
{
    // Sondea EVENTOS_DOCUMENTO (vía ms-facturación) desde el checkpoint de cada empresa (TABLA_MAESTRA
    // IdMaestro=76) y aplica a PEDIDO_FACTURA el resultado de una Comunicación de Baja o de un Resumen
    // Diario de Baja de Boletas — el único camino que sendBill no resuelve en la misma llamada
    // (SP_PedidoFactura_RegistrarEnvio ya cubre Aprobado/Rechazado síncronos al emitir). Transiciones
    // mapeadas: ComunicacionBajaAceptada/ResumenBajaAceptado (EsAnulacion=1) → 8 Anulación Aprobada,
    // ComunicacionBajaRechazada/ResumenBajaRechazado (EsAnulacion=1) → 9 Anulación Rechazada. ms-facturación
    // usa estos estados propios (no el genérico "Rechazado", que significa "el documento en sí fue
    // rechazado") justamente porque una baja rechazada no invalida el documento.
    public class SincronizacionFacturacionWorker(
        IServiceScopeFactory scopeFactory, ILogger<SincronizacionFacturacionWorker> logger) : BackgroundService
    {
        private static readonly TimeSpan Intervalo = TimeSpan.FromMinutes(5);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(Intervalo);
            do
            {
                try
                {
                    await SincronizarAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error no controlado en SincronizacionFacturacionWorker.");
                }
            } while (await timer.WaitForNextTickAsync(stoppingToken));
        }

        private static int? MapearEstadoFacturacion(FacturacionEventoDocumento evento) =>
            (evento.EsAnulacion, evento.EstadoCodigo) switch
            {
                (true, "ComunicacionBajaAceptada") => 8,  // Anulación Aprobada
                (true, "ComunicacionBajaRechazada") => 9, // Anulación Rechazada
                (true, "ResumenBajaAceptado") => 8,       // Anulación Aprobada (Boleta)
                (true, "ResumenBajaRechazado") => 9,      // Anulación Rechazada (Boleta)
                _ => null
            };

        private async Task SincronizarAsync(CancellationToken cancellationToken)
        {
            using var scope = scopeFactory.CreateScope();
            var pedidoFacturaDAO = scope.ServiceProvider.GetRequiredService<PedidoFacturaDAO>();
            var facturacionElectronicaService = scope.ServiceProvider.GetRequiredService<FacturacionElectronicaService>();

            var checkpoints = await pedidoFacturaDAO.ObtenerCheckpointsSincronizacionAsync();
            if (checkpoints.IdTipoMensaje != 2 || checkpoints.Result is not List<CheckpointSincronizacionConsulta> lista)
            {
                logger.LogWarning("No se pudieron obtener los checkpoints de sincronización de facturación: {Mensaje}", checkpoints.Mensaje);
                return;
            }

            var checkpointsAAvanzar = new List<(int IdEmpresa, int UltimoIdEvento)>();

            foreach (var checkpoint in lista)
            {
                var nuevoCheckpoint = await SincronizarEmpresaAsync(pedidoFacturaDAO, facturacionElectronicaService, checkpoint, cancellationToken);
                if (nuevoCheckpoint.HasValue)
                {
                    checkpointsAAvanzar.Add((checkpoint.IdEmpresa, nuevoCheckpoint.Value));
                }
            }

            if (checkpointsAAvanzar.Count > 0)
            {
                var resultado = await pedidoFacturaDAO.ActualizarCheckpointSincronizacionAsync(checkpointsAAvanzar);
                if (resultado.IdTipoMensaje != 2)
                {
                    logger.LogError("No se pudieron avanzar los checkpoints de sincronización de facturación: {Mensaje}", resultado.Mensaje);
                }
            }
        }

        // Devuelve el nuevo checkpoint a fijar para la empresa, o null si no debe avanzar (falla de red/aplicación,
        // se reintenta desde el mismo punto en el próximo ciclo).
        private async Task<int?> SincronizarEmpresaAsync(
            PedidoFacturaDAO pedidoFacturaDAO, FacturacionElectronicaService facturacionElectronicaService,
            CheckpointSincronizacionConsulta checkpoint, CancellationToken cancellationToken)
        {
            var envelope = await facturacionElectronicaService.ListarEventosRecientesAsync(
                checkpoint.IdEmpresa, checkpoint.UltimoIdEvento, cancellationToken);

            if (envelope is null || envelope.IdTipoMensaje != 2)
            {
                logger.LogWarning(
                    "No se pudieron obtener eventos recientes de facturación para la empresa {IdEmpresa}: {Mensaje}",
                    checkpoint.IdEmpresa, envelope?.Mensaje);
                return null;
            }

            var eventos = envelope.Datos ?? [];
            if (eventos.Count == 0)
            {
                return null;
            }

            var documentosConEstado = eventos
                .Select(evento => (Evento: evento, Estado: MapearEstadoFacturacion(evento)))
                .Where(x => x.Estado.HasValue)
                .GroupBy(x => x.Evento.IdDocumentoElectronico)
                .Select(g => (IdDocumentoElectronico: g.Key, IdEstadoFacturacion: g.Last().Estado!.Value))
                .ToList();

            if (documentosConEstado.Count > 0)
            {
                var resultado = await pedidoFacturaDAO.ActualizarEstadoPorDocumentoAsync(checkpoint.IdEmpresa, documentosConEstado);
                if (resultado.IdTipoMensaje != 2)
                {
                    logger.LogError(
                        "No se pudo aplicar el estado de facturación sincronizado para la empresa {IdEmpresa}: {Mensaje}",
                        checkpoint.IdEmpresa, resultado.Mensaje);
                    return null; // No avanza el checkpoint: se reintenta en el próximo ciclo.
                }
            }

            return eventos.Max(evento => evento.IdEventoDocumento);
        }
    }
}
