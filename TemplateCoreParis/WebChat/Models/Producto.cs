using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TemplateCoreParis.WebChat.Models
{
    public class Producto
    {
        public int ProductoID { get; set; }

        [Required]
        public string Codigo { get; set; }

        [Required]
        public string Nombre { get; set; }

        public string ImagenRuta { get; set; }

        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
    }
}