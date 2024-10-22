using System.ComponentModel.DataAnnotations;

namespace TP_PWEB.ViewModels
{
    public class UserManagerViewModel
    {
        public string UserId { get; set; }

        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        public string CompanyName { get; set; }
        public bool Active { get; set; }
        public string UserName { get; set; }
        public IEnumerable<string> RolesNames { get; set; }

        public List<RolesData> ListRoles { get; set; }
    }

    public class RolesData
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public bool Selected { get; set; }
    }
}
