namespace SafetyReport.Application.Puertos.Informe
{
    public class InformeTranslationRequest
    {
        public string Idioma { get; set; } = "";
        public InformeTranslationContent Contenido { get; set; } = new();
    }

    public class InformeTranslationContent
    {
        public Dictionary<string, string?> Identificacion { get; set; } = new();
        public Dictionary<string, string?> Legales { get; set; } = new();
        public InformeTranslationRamoOperaciones RamoOperaciones { get; set; } = new();
        public Dictionary<string, string?> InformacionFinanciera { get; set; } = new();
        public Dictionary<string, string?> BancosProveedores { get; set; } = new();
        public Dictionary<string, string?> DatosGenerales { get; set; } = new();
        public List<InformeTranslationDirectorioEjecutivo> DirectoriosEjecutivos { get; set; } = new();
    }

    public class InformeTranslationDirectorioEjecutivo
    {
        public int? IdInformeDirectorioEjecutivo { get; set; }
        public string? Cargo { get; set; }
        public string? VinculadoDesde { get; set; }
        public string? Referencias { get; set; }
    }

    public class InformeTranslationRamoOperaciones
    {
        public Dictionary<string, string?> Campos { get; set; } = new();
        public Dictionary<string, string?> Importaciones { get; set; } = new();
        public Dictionary<string, string?> Exportaciones { get; set; } = new();
        public List<InformeTranslationLocal> Locales { get; set; } = new();
    }

    public class InformeTranslationLocal
    {
        public int? IdInformeLocal { get; set; }
        public string? TipoLocal { get; set; }
        public string? Comentario { get; set; }
        public List<InformeTranslationLocalImagen> Imagenes { get; set; } = new();
    }

    public class InformeTranslationLocalImagen
    {
        public int? IdInformeLocalImagen { get; set; }
        public string? Descripcion { get; set; }
    }
}
