namespace SafetyReport.Application.Puertos.Almacenamiento;

public interface IInformeArchivoStorage
{
    string GenerarUploadUrl(string rutaArchivo, string formatoArchivo);
    string GenerarDownloadUrl(string rutaArchivo);
    Task DeleteFileAsync(string rutaArchivo);
}
