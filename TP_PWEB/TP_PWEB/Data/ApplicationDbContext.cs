using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TP_PWEB.Models;

namespace TP_PWEB.Data
{
    public class ApplicationDbContext : IdentityDbContext <ApplicationUser>
    {
        public DbSet<Company> Company { get; set; }
        public DbSet<Reservation> Reservation { get; set; }
        public DbSet<Vehicle> Vehicle { get; set; }
        public DbSet<VehicleCategory> VehicleCategory { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}