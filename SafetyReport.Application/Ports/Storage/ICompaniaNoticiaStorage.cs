namespace SafetyReport.Application.Ports.Storage;

public interface ICompaniaNoticiaStorage
{
    string GenerarUploadUrl(string rutaArchivo, string formatoArchivo);
    string GenerarDownloadUrl(string rutaArchivo, string nombreDescarga);
    Task DeleteFileAsync(string rutaArchivo);
}
