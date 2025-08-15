using Microsoft.AspNetCore.Identity;

namespace HotelBookingSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public virtual ICollection<Booking> Bookings { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
    }
}
