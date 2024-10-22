using TP_PWEB.Models;

namespace TP_PWEB.ViewModels
{
    public class CompanyDetailsViewModel
    {
        public Company Company { get; set; }

        public int TotalVehicles { get; set; }

        public List<ApplicationUser> Menagers { get; set; }
        public List<ApplicationUser> Workers { get; set; }
    }
}
