using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TP_PWEB.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        public bool Active { get; set; } = true;

        public int? CompanyID { get; set; }
        public Company Company { get; set; }
    }
}
