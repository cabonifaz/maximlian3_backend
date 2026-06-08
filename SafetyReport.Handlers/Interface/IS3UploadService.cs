using Microsoft.AspNetCore.Http;
using SafetyReport.Models;

public interface IS3UploadService
{
    string GenerarRutaPedidoArchivo(int idPedido, string nombreArchivo,  int idArchivo);
    string GenerarUploadUrl(string rutaArchivo, string formatoArchivo);
    string GenerarDownloadUrl(string rutaArchivo);
    List<string> GenerarUploadUrlsBatch(List<string> sufijos, string formatoArchivo);
    List<string> GenerarDownloadUrlsBatch(List<string> sufijos);
    Task UploadFileAsync(string rutaArchivo, IFormFile file);
    Task DeleteFileAsync(string rutaArchivo);
    Task MoverArchivoAsync(string rutaOrigen, string rutaDestino);
}