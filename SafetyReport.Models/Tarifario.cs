using System;
using System.Collections.Generic;
using System.Text;

namespace SafetyReport.Models
{
    public class TarifarioCrear
    {
        public int IdCliente { get; set; }
        public int IdProducto { get; set; }
        public int IdTipoTramite { get; set; }
        public int IdPais { get; set; }
        public int IdMoneda { get; set; }
        public int DiasMax { get; set; }
        public int DiasMin { get; set; }
        public decimal Precio { get; set; }
        public decimal Penalidad { get; set; }
    }

    public class TarifarioEditar
    {
        public int IdTarifario { get; set; }
        public int IdCliente { get; set; }
        public int IdProducto { get; set; }
        public int IdTipoTramite { get; set; }
        public int IdPais { get; set; }
        public int IdMoneda { get; set; }
        public int DiasMax { get; set; }
        public int DiasMin { get; set; }
        public decimal Precio { get; set; }
        public decimal Penalidad { get; set; }
    }

    public class TarifarioConsulta
    {
        public int IdTarifario { get; set; }
        public int IdCliente { get; set; }
        public int IdProducto { get; set; }
        public int IdTipoTramite { get; set; }
        public int IdPais { get; set; }
        public int IdMoneda { get; set; }
        public int DiasMax { get; set; }
        public int DiasMin { get; set; }
        public decimal Precio { get; set; }
        public decimal Penalidad { get; set; }
    }

    public class TarifarioCreado
    {
        public int IdTarifario { get; set; }
    }

    public class TarifarioEliminado
    {
        public int IdTarifario { get; set; }
    }

    public class TarifarioIdRequest
    {
        public int IdTarifario { get; set; }
        public int IdCliente { get; set; }
    }

    public class TarifarioFiltro
    {
        public int IdCliente { get; set; }
        public string? Filtro { get; set; }
        public int? NumPag { get; set; }
    }

    public class TarifarioListaConsulta
    {
        public int IdTarifario { get; set; }
        public int IdCliente { get; set; }
        public int IdProducto { get; set; }
        public int IdTipoTramite { get; set; }
        public int IdPais { get; set; }
        public int IdMoneda { get; set; }
        public int DiasMax { get; set; }
        public int DiasMin { get; set; }
        public decimal Precio { get; set; }
        public decimal Penalidad { get; set; }
    }

    public class TarifarioListaResult
    {
        public List<TarifarioListaConsulta> lstTarifario { get; set; } = new();
        public int TotalRegistros { get; set; }
        public int TotalPaginas { get; set; }
    }
}