namespace SafetyReport.Models
{
    public class EvolucionInformesRequest
    {
        public int? idColaborador { get; set; }
        public int? rol { get; set; }
        public DateTime? fechaDesde { get; set; }
        public DateTime? fechaHasta { get; set; }
        public int granularidad { get; set; } // 1=Dia, 2=Semana, 3=Mes, 4=Ano
    }

    public class EvolucionInformesConsulta
    {
        public string Periodo { get; set; } = string.Empty;
        public string Etiqueta { get; set; } = string.Empty;
        public int CantidadInformes { get; set; }
    }
}
