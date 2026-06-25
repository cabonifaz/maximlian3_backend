namespace SafetyReport.Models
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
    }

    public class InformeTranslationRamoOperaciones
    {
        public Dictionary<string, string?> Campos { get; set; } = new();
        public Dictionary<string, string?> Importaciones { get; set; } = new();
        public Dictionary<string, string?> Exportaciones { get; set; } = new();
    }
}
