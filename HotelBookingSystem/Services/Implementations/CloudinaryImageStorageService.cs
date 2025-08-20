using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using HotelBookingSystem.Infrastructure.Options;
using HotelBookingSystem.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Net;

namespace HotelBookingSystem.Services.Implementations
{
    public class CloudinaryImageStorageService : IImageStorageService
    {
        private static readonly string[] AllowedContentTypes =
            { "image/jpeg", "image/png", "image/webp", "image/gif" };

        private readonly Cloudinary _cloudinary;
        private readonly CloudinarySettings _settings;

        public CloudinaryImageStorageService(
            Cloudinary cloudinary,
            IOptions<CloudinarySettings> settings)
        {
            _cloudinary = cloudinary;
            _settings = settings.Value;
        }

        public async Task<ImageUploadResultDto> UploadRoomImage(IFormFile file, CancellationToken ct = default)
        {
            if (file is null || file.Length == 0)
                throw new ArgumentException("No file uploaded.");

            if (!AllowedContentTypes.Contains(file.ContentType))
                throw new InvalidOperationException("Unsupported image type.");

            await using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = string.IsNullOrWhiteSpace(_settings.Folder) ? "rooms" : _settings.Folder,
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false,

                // Helpful defaults:
                Transformation = new Transformation()
                    .Quality("auto")
                    .FetchFormat("auto")
                    .Dpr("auto")
            };

            var result = await _cloudinary.UploadAsync(uploadParams, ct);

            if (result.StatusCode != HttpStatusCode.OK || result.Error != null)
                throw new InvalidOperationException(result.Error?.Message ?? "Cloudinary upload failed.");

            return new ImageUploadResultDto(
                Url: result.SecureUrl?.ToString() ?? "",
                PublicId: result.PublicId,
                Width: result.Width,
                Height: result.Height,
                Format: result.Format
            );
        }

        public async Task Delete(string publicId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(publicId)) return;
            var delParams = new DeletionParams(publicId);
            await _cloudinary.DestroyAsync(delParams);
        }
    }
}
