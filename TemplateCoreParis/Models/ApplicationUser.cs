using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace TemplateCoreParis.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
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
    }
}
