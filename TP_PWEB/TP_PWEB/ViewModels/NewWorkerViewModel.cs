using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace TP_PWEB.ViewModels
{
    public class NewWorkerViewModel
    {
        [Display(Name = "Email")]
        public string Email { get; set; }
        [Display(Name = "First Name")]
        public string PrimeiroNome { get; set; }
        [Display(Name = "Last Name")]
        public string UltimoNome { get; set; }
        public bool Ativo { get; set; } = true;

        [Required]
        [StringLength(100, ErrorMessage = "A {0} deve ter entre {2} a {1} caracteres.", MinimumLength = 6)]
        public string Password { get; set; }

        public string NomeEmpresa { get; set; }

        public List<SelectListItem> ListaCargos = new List<SelectListItem> {
            new SelectListItem { Text = "Worker", Value = "Worker" },
            new SelectListItem { Text = "Manager", Value = "Manager" }
        };
    }
}
