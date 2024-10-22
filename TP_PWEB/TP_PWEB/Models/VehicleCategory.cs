using System.ComponentModel.DataAnnotations;

namespace TP_PWEB.Models
{
    public class VehicleCategory
    {
        public int Id { get; set; }

        [Display(Name = "type of vehicle")]
        public string Name { get; set; }

        public ICollection<Vehicle> Vehicles { get; set; }
    }
}
