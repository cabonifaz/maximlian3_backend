namespace SafetyReport.Models
{
    public class ResumenClientesDashboard
    {
        public int TotalClientes { get; set; }
        public int TotalActivos { get; set; }
        public int TotalInactivos { get; set; }
        public decimal? PorcentajeActivos { get; set; }
        public decimal? PorcentajeCrecimiento { get; set; }
        public DateTime FechaActualizacion { get; set; }
    }
}
