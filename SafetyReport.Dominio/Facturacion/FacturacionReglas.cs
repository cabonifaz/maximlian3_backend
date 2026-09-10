namespace SafetyReport.Domain.Facturacion;

public static class FacturacionReglas
{
    private const string CodigoBoleta = "03";

    public static int? MapearEstadoFacturacionSunat(int estadoCodigoSunat)
    {
        return estadoCodigoSunat switch
        {
            3 or 4 or 5 or 8 => estadoCodigoSunat,
            _ => null
        };
    }

    public static bool EsBoleta(string? tipoDocumentoCodigo, string? tipoDocumentoRelacionadoCodigo)
    {
        return tipoDocumentoCodigo == CodigoBoleta || tipoDocumentoRelacionadoCodigo == CodigoBoleta;
    }
}
