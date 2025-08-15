namespace HotelBookingSystem.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; } // Credit Card, PayPal, Bank Transfer
        public string TransactionId { get; set; }

        public int PaymentStatusId { get; set; }
        public virtual PaymentStatus PaymentStatus { get; set; }

        public int BookingId { get; set; }
        public virtual Booking Booking { get; set; }
    }
}
