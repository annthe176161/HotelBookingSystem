namespace HotelBookingSystem.Infrastructure.Options
{
    public class CloudinarySettings
    {
        public string CloudName { get; set; } = default!;
        public string ApiKey { get; set; } = default!;
        public string ApiSecret { get; set; } = default!;
        public string? Folder { get; set; }
    }
}
