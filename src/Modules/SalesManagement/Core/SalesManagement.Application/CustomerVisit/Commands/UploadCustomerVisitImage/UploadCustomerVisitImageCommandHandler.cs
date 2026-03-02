using Contracts.Interfaces.Lookups.Users;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.CustomerVisit.Dto;

namespace SalesManagement.Application.CustomerVisit.Commands.UploadCustomerVisitImage
{
    public class UploadCustomerVisitImageCommandHandler : IRequestHandler<UploadCustomerVisitImageCommand, CustomerVisitImageDto>
    {
        private readonly IFileUploadService _fileUploadService;
        private readonly IIPAddressService _ipAddressService;
        private readonly ICompanyLookup _companyLookup;
        private readonly IUnitLookup _unitLookup;
        private readonly ILogger<UploadCustomerVisitImageCommandHandler> _logger;

        public UploadCustomerVisitImageCommandHandler(
            IFileUploadService fileUploadService,
            IIPAddressService ipAddressService,
            ICompanyLookup companyLookup,
            IUnitLookup unitLookup,
            ILogger<UploadCustomerVisitImageCommandHandler> logger)
        {
            _fileUploadService = fileUploadService;
            _ipAddressService = ipAddressService;
            _companyLookup = companyLookup;
            _unitLookup = unitLookup;
            _logger = logger;
        }

        public async Task<CustomerVisitImageDto> Handle(UploadCustomerVisitImageCommand request, CancellationToken cancellationToken)
        {
            if (request.File == null || request.File.Length == 0)
            {
                throw new ValidationException("File is required.");
            }

            // Get company and unit context
            var companyId = _ipAddressService.GetCompanyId();
            var unitId = _ipAddressService.GetUnitId();

            var companies = await _companyLookup.GetAllCompanyAsync();
            var companyName = companies.FirstOrDefault(c => c.CompanyId == companyId)?.CompanyName ?? "Default";

            var units = await _unitLookup.GetAllUnitAsync();
            var unitName = units.FirstOrDefault(u => u.UnitId == unitId)?.UnitName ?? "Default";

            // Construct upload path
            var uploadPath = Path.Combine("Resources", "CustomerVisit", companyName, unitName);

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
