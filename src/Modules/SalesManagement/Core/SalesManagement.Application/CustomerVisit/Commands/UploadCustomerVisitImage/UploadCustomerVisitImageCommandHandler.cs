using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using SalesManagement.Application.CustomerVisit.Dto;
using SalesManagement.Domain.Common;

namespace SalesManagement.Application.CustomerVisit.Commands.UploadCustomerVisitImage
{
    public class UploadCustomerVisitImageCommandHandler : IRequestHandler<UploadCustomerVisitImageCommand, CustomerVisitImageDto>
    {
        private readonly ILogger<UploadCustomerVisitImageCommandHandler> _logger;

        public UploadCustomerVisitImageCommandHandler(
            ILogger<UploadCustomerVisitImageCommandHandler> logger)
        {
            _logger = logger;
        }

        public async Task<CustomerVisitImageDto> Handle(UploadCustomerVisitImageCommand request, CancellationToken cancellationToken)
        {
            if (request.File == null || request.File.Length == 0)
            {
                throw new ValidationException("File is required.");
            }

            // Construct upload path
            var uploadPath = Path.Combine("Resources", MiscEnumEntity.CustomerVisit);

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // Generate unique filename
            var extension = Path.GetExtension(request.File.FileName);
            var fileName = $"TEMP_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadPath, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.File.CopyToAsync(stream, cancellationToken);
            }

            // Convert to Base64
            string? imageBase64 = null;
            if (File.Exists(filePath))
            {
                var imageBytes = await File.ReadAllBytesAsync(filePath, cancellationToken);
                imageBase64 = Convert.ToBase64String(imageBytes);
            }

            _logger.LogInformation("CustomerVisit image uploaded: {FileName} at {FilePath}", fileName, filePath);

            return new CustomerVisitImageDto
            {
                ImageName = fileName,
                ImageBase64 = imageBase64
            };
        }
    }
}
