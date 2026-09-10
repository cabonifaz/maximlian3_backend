namespace SafetyReport.Application.Puertos.Almacenamiento;

public interface IPedidoArchivoStorage
{
    string GenerarRutaPedidoArchivo(int idPedido, string nombreArchivo, int idArchivo);
    string GenerarUploadUrl(string rutaArchivo, string formatoArchivo);
    string GenerarDownloadUrl(string rutaArchivo);
    Task DeleteFileAsync(string rutaArchivo);
    Task MoverArchivoAsync(string rutaOrigen, string rutaDestino);
}
