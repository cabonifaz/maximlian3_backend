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
        public int idTarifario { get; set; }
        public int idCliente { get; set; }
    }

    public class TarifarioFiltro
    {
        public int idCliente { get; set; }
        public string? busqueda { get; set; }
        public int? numPag { get; set; }
    }

    public class TarifarioListaConsulta
    {
        public int IdTarifario { get; set; }
        public string Producto { get; set; } = string.Empty;
        public string Pais { get; set; } = string.Empty;
        public string Moneda { get; set; } = string.Empty;
        public string TipoTramite { get; set; } = string.Empty;
        public string DiasMinMax { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public decimal Penalidad { get; set; }
    }

    public class TarifarioListaResult
    {
        public List<TarifarioListaConsulta> lstTarifario { get; set; } = new();
        public int TotalRegistros { get; set; }
        public int TotalPaginas { get; set; }
    }
    public class TarifarioListaCortaFiltro
    {
        public int idCliente { get; set; }
        public int? idTipoProducto { get; set; }
        public int? idTipoTramite { get; set; }
        public int? idPais { get; set; }
    }

    public class TarifarioListaCorta
    {
        public int IdTarifario { get; set; }
        public string TipoTramite { get; set; } = string.Empty;
        public int IdMoneda { get; set; }
        public string Moneda { get; set; } = string.Empty;
        public string SimboloMoneda { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int IdPais { get; set; }
        public string Pais { get; set; } = string.Empty;
        public int IdProducto { get; set; }
        public int IdTipoTramite { get; set; }
        public int DiasMin { get; set; }
        public int DiasMax { get; set; }
    }
    public class TarifarioListaCortaResult
    {
        public List<TarifarioListaCorta> lstTarifario { get; set; } = new();
    }
}