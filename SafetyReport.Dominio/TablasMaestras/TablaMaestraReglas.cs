namespace SafetyReport.Domain.TablasMaestras;

public static class TablaMaestraReglas
{
    private static readonly HashSet<int> MaestrosSoloString1 = new()
    {
        14,
        44,
        45,
        47,
        48,
        49,
        52,
        56,
        57,
        58,
        59,
        60,
        61
    };

    public static bool UsaSoloString1(int idMaestro)
    {
        return MaestrosSoloString1.Contains(idMaestro);
    }
}
