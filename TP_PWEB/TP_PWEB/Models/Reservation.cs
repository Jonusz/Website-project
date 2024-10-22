using System.ComponentModel.DataAnnotations;

namespace TP_PWEB.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public DateTime PickUpDate { get; set; }
        public DateTime DeliveryDate { get; set; }
        public decimal Price { get; set; }
        public decimal? Kms { get; set; }
        public bool DemageInPickup { get; set; } = false;
        public bool DemageInDelivery { get; set; } = false;
        public string? Observations { get; set; }
        [Range(0, 5, ErrorMessage = "Clasyfication in scale from 0 to 5")]
        public int? Clasyfication { get; set; } // 0, 1, 2, 3, 4, 5
        public int ReservationState { get; set; } = 0; // -1: rejeted | 0: pending | 1: accepted | 2: closed

        public string ClientId { get; set; }
        public ApplicationUser Client { get; set; }

        public string? DeliveryWorkerId { get; set; }
        public ApplicationUser DeliveryWorker { get; set; }

        public string? PickUpWorkerID { get; set; }
        public ApplicationUser PickUpWorker { get; set; }

        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }
    }
}
