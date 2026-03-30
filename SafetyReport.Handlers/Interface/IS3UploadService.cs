using Microsoft.AspNetCore.Http;
using SafetyReport.Models;

public interface IS3UploadService
{
    string GenerarRutaPedidoArchivo(int idPedido, string nombreArchivo,  int idArchivo);
    string GenerarUploadUrl(string rutaArchivo, string formatoArchivo);
    string GenerarDownloadUrl(string rutaArchivo);
    Task UploadFileAsync(string rutaArchivo, IFormFile file);
    Task DeleteFileAsync(string rutaArchivo);
    Task MoverArchivoAsync(string rutaOrigen, string rutaDestino);
}