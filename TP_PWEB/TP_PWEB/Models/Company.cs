using System.ComponentModel.DataAnnotations;

namespace TP_PWEB.Models
{
    public class Company
    {
        public int Id { get; set; }

        [Display(Name = "Company")]
        public string Name { get; set; }

        [Display(Name = "Rating")]
        [Range(0.0, 5.0, ErrorMessage = "Rating for an company in scale from 0 to 5")]
        public decimal? rating { get; set; }
        public int? NumberOfRatings { get; set; } 

        [Display(Name = "Active Company")]
        public bool Active { get; set; }

        public ICollection<Vehicle> Vehicles { get; set; }
        public ICollection<ApplicationUser> Workers { get; set; }
    }
}
