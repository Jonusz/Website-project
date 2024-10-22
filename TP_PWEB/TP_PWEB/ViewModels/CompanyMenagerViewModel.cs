using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using TP_PWEB.Models;

namespace TP_PWEB.ViewModels
{
    public class CompanyMenagerViewModel
    {
        //New Company
        public int IdEmpresa { get; set; }
        [Display(Name = "Company")]
        public string NomeEmpresa { get; set; }
        public bool AtivoEmpresa { get; set; }

        //Manager with Company
        [Display(Name = "Email")]
        public string EmailGestor { get; set; }
        [Display(Name = "First Name")]
        public string PrimeiroNomeGestor { get; set; }
        [Display(Name = "Last Name")]
        public string UltimoNomeGestor { get; set; }
        public bool AtivoGestor { get; set; } = true;

        [Required]
        [StringLength(100, ErrorMessage = "A {0} deve ter entre {2} a {1} caracteres.", MinimumLength = 6)]
        public string PasswordGestor { get; set; }
    }
}
