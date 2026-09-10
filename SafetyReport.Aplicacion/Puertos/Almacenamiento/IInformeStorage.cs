namespace SafetyReport.Application.Puertos.Almacenamiento;

public interface IInformeStorage
{
    string GenerarUploadUrl(string rutaArchivo, string formatoArchivo);
    string GenerarDownloadUrl(string rutaArchivo);
    string GenerarDownloadUrl(string rutaArchivo, string nombreDescarga);
    List<string> GenerarDownloadUrlsBatch(List<string> sufijos);
    Task UploadStreamAsync(string rutaArchivo, Stream stream, string contentType);
    Task<byte[]?> DescargarBytesAsync(string rutaArchivo);
    Task CopiarArchivoAsync(string rutaOrigen, string rutaDestino);
}
