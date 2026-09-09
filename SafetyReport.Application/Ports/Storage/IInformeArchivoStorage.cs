namespace SafetyReport.Application.Ports.Storage;

public interface IInformeArchivoStorage
{
    string GenerarUploadUrl(string rutaArchivo, string formatoArchivo);
    string GenerarDownloadUrl(string rutaArchivo);
    Task DeleteFileAsync(string rutaArchivo);
}
