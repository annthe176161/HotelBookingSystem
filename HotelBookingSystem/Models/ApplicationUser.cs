using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace HotelBookingSystem.Models
{
    public enum GenderType { Unknow = 0, Name = 1, Nu = 2 }
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(150)]
        public string FullName { get; set; }
        public bool IsActivated { get; set; } = true;

        public virtual ICollection<Booking>? Bookings { get; set; }
        public virtual ICollection<Review>? Reviews { get; set; }
        [MaxLength(100)]
        public string FirstName { get; set; }
        [MaxLength(100)]
        public string LastName { get; set; }
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }
        public GenderType Gender { get; set; } = GenderType.Unknow;
        [MaxLength(200)]
        public string? Address { get; set; }
        [MaxLength(100)]
        public string? City { get; set; }
        [MaxLength(100)]
        public string? State { get; set; }
        [MaxLength(20)]
        public string? ZipCode { get; set; }
        [MaxLength(500)]
        public string? ProfilePictureUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Booking> Bookings { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
    }
}
