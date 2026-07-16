using System;
using System.Collections.Generic;
using System.Text;

namespace SafetyReport.Models
{
    public class Respuesta
    {
        public int IdTipoMensaje { get; set; }

        public string Mensaje { get; set; }

        public object? Result { get; set; }
    }
}
