using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace TemplateCoreParis.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [Display(Name = "Nombres")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Apellidos")]
        public string LastName { get; set; }

        [Display(Name = "DNI")]
        public int DocIdentity { get; set; }

        [Display(Name = "Cargo")]
        public string Title { get; set; }

        [Display(Name = "Cumpleaños")]
        public DateTime? Birthday { get; set; }

        [Display(Name = "Pregunta Secreta")]
        public string SecretQuestion { get; set; }

        [Display(Name = "Respuesta Secreta")]
        public string SecretResponse { get; set; }

    }
}
