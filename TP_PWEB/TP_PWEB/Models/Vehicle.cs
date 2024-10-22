using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace TP_PWEB.Models
{
    public class Vehicle
    {
        public int Id { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        [Range(2, 9, ErrorMessage = "Number of seats must be from 2 to 9")]
        public int Seats { get; set; }
        public decimal Price { get; set; }
        public bool Available { get; set; } = true;
        public decimal Kms { get; set; } 
        public string Location { get; set; }

        public int CompanyId { get; set; }
        public Company Company { get; set; }

        public int CategoryID { get; set; }
        public VehicleCategory Category { get; set; }
    }
}
