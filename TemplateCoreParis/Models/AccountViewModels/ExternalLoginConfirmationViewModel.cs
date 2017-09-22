using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TemplateCoreParis.Models.AccountViewModels
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "Nombres")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Apellidos")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Teléfono")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Cumpleaños")]
        public DateTime? Birthday { get; set; }

        [Required]
        [Display(Name = "Pregunta Secreta")]
        public string SecretQuestion { get; set; }

        [Required]
        [Display(Name = "Respuesta Secreta")]
        public string SecretResponse { get; set; }
    }
}
