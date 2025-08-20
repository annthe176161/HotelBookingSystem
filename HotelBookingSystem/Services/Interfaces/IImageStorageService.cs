namespace HotelBookingSystem.Services.Interfaces
{
    public record ImageUploadResultDto(
        string Url,
        string PublicId,
        int? Width,
        int? Height,
        string? Format);

    public interface IImageStorageService
    {
        Task<ImageUploadResultDto> UploadRoomImage(IFormFile file, CancellationToken ct = default);
        Task Delete(string publicId, CancellationToken ct = default);
    }
}
