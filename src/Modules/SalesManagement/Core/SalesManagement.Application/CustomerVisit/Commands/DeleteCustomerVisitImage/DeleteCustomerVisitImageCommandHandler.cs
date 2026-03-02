using Contracts.Interfaces.Lookups.Users;
using MediatR;
using Microsoft.Extensions.Logging;
using SalesManagement.Application.Common.Interfaces;

namespace SalesManagement.Application.CustomerVisit.Commands.DeleteCustomerVisitImage
{
    public class DeleteCustomerVisitImageCommandHandler : IRequestHandler<DeleteCustomerVisitImageCommand, bool>
    {
        private readonly IFileUploadService _fileUploadService;
        private readonly IIPAddressService _ipAddressService;
        private readonly ICompanyLookup _companyLookup;
        private readonly IUnitLookup _unitLookup;
        private readonly ILogger<DeleteCustomerVisitImageCommandHandler> _logger;

        public DeleteCustomerVisitImageCommandHandler(
            IFileUploadService fileUploadService,
            IIPAddressService ipAddressService,
            ICompanyLookup companyLookup,
            IUnitLookup unitLookup,
            ILogger<DeleteCustomerVisitImageCommandHandler> logger)
        {
            _fileUploadService = fileUploadService;
            _ipAddressService = ipAddressService;
            _companyLookup = companyLookup;
            _unitLookup = unitLookup;
            _logger = logger;
        }

        public async Task<bool> Handle(DeleteCustomerVisitImageCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.ImagePath))
                return false;

            // Get company and unit context
            var companyId = _ipAddressService.GetCompanyId();
            var unitId = _ipAddressService.GetUnitId();

            var companies = await _companyLookup.GetAllCompanyAsync();
            var companyName = companies.FirstOrDefault(c => c.CompanyId == companyId)?.CompanyName ?? "Default";

            var units = await _unitLookup.GetAllUnitAsync();
            var unitName = units.FirstOrDefault(u => u.UnitId == unitId)?.UnitName ?? "Default";

            // Construct full file path
            var fullPath = Path.Combine("Resources", "CustomerVisit", companyName, unitName, request.ImagePath);

            var result = await _fileUploadService.DeleteFileAsync(fullPath);

            _logger.LogInformation("CustomerVisit image deleted: {ImagePath}, Success: {Result}", request.ImagePath, result);

            return result;
        }
    }
}
