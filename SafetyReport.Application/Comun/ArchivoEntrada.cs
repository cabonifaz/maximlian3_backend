namespace SafetyReport.Application.Comun
{
    public class ArchivoEntrada
    {
        public string NombreArchivo { get; set; } = string.Empty;
        public string TipoContenido { get; set; } = string.Empty;
        public Stream Contenido { get; set; } = Stream.Null;
        public long TamanoBytes { get; set; }
    }
}
