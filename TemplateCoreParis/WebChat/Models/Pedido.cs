using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TemplateCoreParis.WebChat.Models
{
    public class Pedido
    {
        public int PedidoID { get; set; }

        [Required]
        public string NroPedido { get; set; }

        [Required]
        public string Nombre { get; set; }

        public string FechaPedido { get; set; }
        public string HoraPedido { get; set; }

        public string FechaEntrega { get; set; }
        public string HoraEntrega { get; set; }

        public string EmpresaEntrega { get; set; }

    }
}