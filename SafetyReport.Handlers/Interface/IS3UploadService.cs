using Microsoft.AspNetCore.Http;
using SafetyReport.Models;

public interface IS3UploadService
{
    string GenerarRutaPedidoArchivo(int idPedido, string nombreArchivo);
    string GenerarUploadUrl(string rutaArchivo, string tipoArchivo);
    Task UploadFileAsync(string rutaArchivo, IFormFile file);
}