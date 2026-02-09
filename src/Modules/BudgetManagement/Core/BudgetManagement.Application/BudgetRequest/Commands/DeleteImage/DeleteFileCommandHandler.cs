using Contracts.Interfaces.Lookups.Users;
using BudgetManagement.Application.Common.Interfaces;
using BudgetManagement.Application.Common.Interfaces.IBudgetRequest;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BudgetManagement.Application.BudgetRequest.Commands.DeleteImage
{
    public class DeleteFileCommandHandler : IRequestHandler<DeleteFileCommand, bool>
    {
        private readonly IBudgetRequestQueryRepository _budgetRequestQueryRepository;
        private readonly IBudgetRequestCommandRepository _budgetRequestCommandRepository;
        private readonly ILogger<DeleteFileCommandHandler> _logger;
        private readonly IFileUploadService _fileUploadService;
        private readonly IIPAddressService _ipAddressService;
        private readonly IUnitLookup _unitLookup;
        private readonly ICompanyLookup _companyLookup;

        public DeleteFileCommandHandler(
            ILogger<DeleteFileCommandHandler> logger,
            IBudgetRequestQueryRepository budgetRequestQueryRepository,
            IFileUploadService fileUploadService,
            IBudgetRequestCommandRepository budgetRequestCommandRepository,
            IIPAddressService ipAddressService,
            IUnitLookup unitLookup,
            ICompanyLookup companyLookup)
        {
            _logger = logger;
            _budgetRequestQueryRepository = budgetRequestQueryRepository;
            _fileUploadService = fileUploadService;
            _budgetRequestCommandRepository = budgetRequestCommandRepository;
            _ipAddressService = ipAddressService;
            _unitLookup = unitLookup;
            _companyLookup = companyLookup;
        }

        public async Task<bool> Handle(DeleteFileCommand request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId();
            var unitId = _ipAddressService.GetUnitId();
            var companies = await _companyLookup.GetAllCompanyAsync();
            var units = await _unitLookup.GetAllUnitAsync();

            var companyLookupDict = companies.ToDictionary(c => c.CompanyId, c => c.CompanyName);
            var unitLookupDict = units.ToDictionary(u => u.UnitId, u => u.UnitName);

            var companyName = companyLookupDict.TryGetValue(companyId, out var cname) ? cname : string.Empty;
            var unitName = unitLookupDict.TryGetValue(unitId, out var uname) ? uname : string.Empty;   

            string baseDirectory = await _budgetRequestQueryRepository.GetBaseDirectoryAsync();
            if (string.IsNullOrWhiteSpace(baseDirectory))
            {
                _logger.LogError("Base directory path not found in database.");
                throw new Exception("Base directory not configured.");                            
            }            
            string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", baseDirectory,companyName,unitName);       

            string filePath = Path.Combine(uploadPath, request.imagePath??string.Empty);

            var result = await _fileUploadService.DeleteFileAsync(filePath);

            await _budgetRequestCommandRepository.RemoveImageReferenceAsync(request.imagePath);
            if (result)
            {
                return result;
            }
            throw new Exception("File deletion failed");            
        }
    }
}
