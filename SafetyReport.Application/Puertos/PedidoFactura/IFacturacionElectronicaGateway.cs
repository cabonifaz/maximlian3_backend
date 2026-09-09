using SafetyReport.Models;

namespace SafetyReport.Application.Puertos.PedidoFactura
{
    public interface IFacturacionElectronicaGateway
    {
        Task<(bool Exito, string Mensaje, byte[]? Contenido, string? NombreArchivo)> ObtenerTxtSireRvieAsync(
            int idInquilino, int idEmpresa, DateOnly periodo, CancellationToken cancellationToken);

        Task<FacturacionEnvelope<FacturacionDocumentoCreado>?> InsertarDocumentoAsync(
            FacturacionInsertarDocumentoRequest request, CancellationToken cancellationToken);

        Task<FacturacionEnvelope<FacturacionResultadoEnvioSunat>?> EnviarASunatAsync(
            int idInquilino, int idDocumentoElectronico, CancellationToken cancellationToken);

        Task<FacturacionEnvelope<object>?> ObtenerDocumentoAsync(
            int idInquilino, int idDocumentoElectronico, CancellationToken cancellationToken);

        Task<FacturacionEnvelope<FacturacionDocumentoTipoLookup>?> ObtenerTipoDocumentoAsync(
            int idInquilino, int idDocumentoElectronico, CancellationToken cancellationToken);

        Task<FacturacionEnvelope<object>?> ObtenerDocumentoPorTokenAsync(
            string token, CancellationToken cancellationToken);

        Task<FacturacionEnvelope<FacturacionIdentificadorPorToken>?> ObtenerIdDocumentoPorTokenAsync(
            string token, CancellationToken cancellationToken);

        Task<FacturacionEnvelope<string>?> ObtenerUrlDescargaPorTokenAsync(
            string token, string tipoArchivo, CancellationToken cancellationToken);

        Task<FacturacionEnvelope<FacturacionDatosParaNota>?> ObtenerParaNotaAsync(
            int idInquilino, int idDocumentoElectronico, CancellationToken cancellationToken);

        Task<FacturacionEnvelope<string>?> ObtenerTokenVerificacionAsync(
            int idInquilino, int idDocumentoElectronico, CancellationToken cancellationToken);

        Task<FacturacionEnvelope<string>?> ObtenerUrlDescargaAsync(
            int idInquilino, int idDocumentoElectronico, string tipoArchivo, CancellationToken cancellationToken);

        Task<FacturacionEnvelope<List<FacturacionErrorDocumento>>?> ObtenerErroresUltimoEnvioAsync(
            int idInquilino, int idDocumentoElectronico, CancellationToken cancellationToken);

        Task<FacturacionEnvelope<int>?> InsertarCampoExtraAsync(
            FacturacionInsertarCampoExtraRequest request, CancellationToken cancellationToken);

        Task<FacturacionEnvelope<List<int>>?> InsertarLoteCamposExtraAsync(
            FacturacionInsertarLoteCamposExtraRequest request, CancellationToken cancellationToken);

        Task<FacturacionEnvelope<List<FacturacionCampoExtra>>?> ListarCamposExtraAsync(
            int idInquilino, int idDocumentoElectronico, CancellationToken cancellationToken);

        Task<FacturacionEnvelope<int>?> ActualizarCampoExtraAsync(
            int idInquilino, int idCampoExtraDocumentoElectronico, FacturacionCampoExtraEntrada campo,
            CancellationToken cancellationToken);

        Task<FacturacionEnvelope<int>?> EliminarCampoExtraAsync(
            int idInquilino, int idCampoExtraDocumentoElectronico, CancellationToken cancellationToken);

        Task<FacturacionEnvelope<FacturacionCuotaActualizada>?> ActualizarEstadoCuotaAsync(
            int idInquilino, int idDocumentoElectronico, int idCuotaDocumentoElectronico,
            int estadoCuotaCodigo, DateTime? fechaPago, CancellationToken cancellationToken);

        Task<FacturacionEnvelope<List<FacturacionEstadoDocumentoActualizado>>?> AnularManualmenteAsync(
            int idInquilino, int idDocumentoElectronico, FacturacionAnularManualmenteRequest request,
            CancellationToken cancellationToken);

        Task<FacturacionEnvelope<List<FacturacionDocumentoAnulacionManualPreview>>?> PrevisualizarAnulacionManualAsync(
            int idInquilino, int idDocumentoElectronico, CancellationToken cancellationToken);

        Task<FacturacionEnvelope<bool>?> EliminarBorradorAsync(
            int idInquilino, int idDocumentoElectronico, CancellationToken cancellationToken);

        Task<FacturacionEnvelope<object>?> GuardarCambiosAsync(
            int idInquilino, int idDocumentoElectronico, FacturacionGuardarCambiosRequest request,
            CancellationToken cancellationToken);

        Task<FacturacionEnvelope<FacturacionLoteDocumentoCreado>?> EnviarComunicacionBajaAsync(
            FacturacionComunicacionBajaRequest request, CancellationToken cancellationToken);

        Task<FacturacionEnvelope<List<FacturacionDocumentoBajaPreview>>?> PrevisualizarBajaAsync(
            int idInquilino, int idEmpresa, IReadOnlyList<int> idsDocumentoElectronico,
            CancellationToken cancellationToken);

        Task<FacturacionEnvelope<FacturacionLoteDocumentoCreado>?> EnviarResumenBajaBoletaAsync(
            FacturacionComunicacionBajaRequest request, CancellationToken cancellationToken);

        Task<FacturacionEnvelope<List<FacturacionDocumentoBajaPreview>>?> PrevisualizarResumenBajaBoletaAsync(
            int idInquilino, int idEmpresa, IReadOnlyList<int> idsDocumentoElectronico,
            CancellationToken cancellationToken);

        Task<FacturacionEnvelope<FacturacionResultadoPaginado<FacturacionFacturaResumen>>?> ListarFacturasAsync(
            int idInquilino, int idEmpresa, string? estadoCodigo, int? idFormaPago, DateOnly? fechaDesde,
            DateOnly? fechaHasta, string? busqueda, int pagina, int tamanoPagina,
            CancellationToken cancellationToken);

        Task<FacturacionEnvelope<FacturacionResumenFacturacion>?> ObtenerResumenAsync(
            int idInquilino, int idEmpresa, DateOnly? fechaDesde, DateOnly? fechaHasta,
            CancellationToken cancellationToken);

        Task<FacturacionEnvelope<FacturacionMontosFacturacion>?> ObtenerMontosFacturacionAsync(
            int idInquilino, int idEmpresa, DateOnly? fechaDesde, DateOnly? fechaHasta,
            CancellationToken cancellationToken);

        Task<FacturacionEnvelope<List<FacturacionDesgloseEstado>>?> ObtenerDesgloseEstadoFacturacionAsync(
            int idInquilino, int idEmpresa, DateOnly? fechaDesde, DateOnly? fechaHasta,
            int? idTipoDocumentoMaestro, CancellationToken cancellationToken);

        Task<FacturacionEnvelope<List<FacturacionEvolucion>>?> ObtenerEvolucionFacturacionAsync(
            int idInquilino, int idEmpresa, DateOnly? fechaDesde, DateOnly? fechaHasta,
            int granularidad, CancellationToken cancellationToken);

        Task<FacturacionEnvelope<List<FacturacionEventoDocumento>>?> ListarEventosRecientesAsync(
            int idInquilino, int ultimoIdEvento, CancellationToken cancellationToken);
    }
}
