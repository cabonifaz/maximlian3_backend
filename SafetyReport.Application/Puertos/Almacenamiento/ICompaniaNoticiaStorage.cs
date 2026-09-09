namespace SafetyReport.Application.Puertos.Almacenamiento;

public interface ICompaniaNoticiaStorage
{
    string GenerarUploadUrl(string rutaArchivo, string formatoArchivo);
    string GenerarDownloadUrl(string rutaArchivo, string nombreDescarga);
    Task DeleteFileAsync(string rutaArchivo);
}
