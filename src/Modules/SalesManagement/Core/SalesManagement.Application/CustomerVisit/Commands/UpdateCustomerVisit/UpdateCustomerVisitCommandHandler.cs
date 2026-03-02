using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using Microsoft.Extensions.Logging;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.Common.Interfaces.ICustomerVisit;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.CustomerVisit.Commands.UpdateCustomerVisit
{
    public class UpdateCustomerVisitCommandHandler : IRequestHandler<UpdateCustomerVisitCommand, ApiResponseDTO<int>>
    {
        private readonly ICustomerVisitCommandRepository _commandRepository;
        private readonly ICustomerVisitQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IIPAddressService _ipAddressService;
        private readonly ICompanyLookup _companyLookup;
        private readonly IUnitLookup _unitLookup;
        private readonly ILogger<UpdateCustomerVisitCommandHandler> _logger;

        public UpdateCustomerVisitCommandHandler(
            ICustomerVisitCommandRepository commandRepository,
            ICustomerVisitQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper,
            IIPAddressService ipAddressService,
            ICompanyLookup companyLookup,
            IUnitLookup unitLookup,
            ILogger<UpdateCustomerVisitCommandHandler> logger)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
            _ipAddressService = ipAddressService;
            _companyLookup = companyLookup;
            _unitLookup = unitLookup;
            _logger = logger;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateCustomerVisitCommand request, CancellationToken cancellationToken)
        {
            // Pre-check: if image is provided, verify file exists on disk
            string? uploadPath = null;
            if (!string.IsNullOrWhiteSpace(request.ImageName))
            {
                uploadPath = await BuildImageUploadPathAsync();
                var filePath = Path.Combine(uploadPath, request.ImageName);

                if (!File.Exists(filePath))
                {
                    return new ApiResponseDTO<int>
                    {
                        IsSuccess = false,
                        Message = $"Image not found at path: {request.ImageName}",
                        Data = 0
                    };
                }
            }

            var entity = _mapper.Map<Domain.Entities.CustomerVisit>(request);

            var result = await _commandRepository.UpdateAsync(entity);

            // Post-save: rename image to {companyName}-{unitName}-{customerVisitId}.ext
            if (!string.IsNullOrWhiteSpace(request.ImageName) && uploadPath != null)
            {
                try
                {
                    await RenameCustomerVisitImageAsync(request.Id, request.ImageName!, uploadPath, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to rename customer visit image for Id {CustomerVisitId}.", request.Id);
                }
            }

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "CUSTOMER_VISIT_UPDATE",
                actionName: request.Id.ToString(),
                details: $"CustomerVisit with Id {request.Id} updated successfully.",
                module: "CustomerVisit"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Customer Visit updated successfully.",
                Data = result
            };
        }

        private async Task<string> BuildImageUploadPathAsync()
        {
            //var baseDirectory = await _queryRepository.GetImageFolderAsync();

            var companies = await _companyLookup.GetAllCompanyAsync();
            var units = await _unitLookup.GetAllUnitAsync();

            var companyName = companies.FirstOrDefault(c => c.CompanyId == _ipAddressService.GetCompanyId())?.CompanyName ?? string.Empty;
            var unitName = units.FirstOrDefault(u => u.UnitId == _ipAddressService.GetUnitId())?.UnitName ?? string.Empty;

            return Path.Combine(
                Directory.GetCurrentDirectory(),
                "Resources",
                "CustomerVisit",
                companyName,
                unitName);
        }

        private async Task RenameCustomerVisitImageAsync(int customerVisitId, string originalFileName, string uploadPath, CancellationToken ct)
        {
            var filePath = Path.Combine(uploadPath, originalFileName);
            if (!File.Exists(filePath))
                return;

            var companies = await _companyLookup.GetAllCompanyAsync();
            var units = await _unitLookup.GetAllUnitAsync();

            var companyName = companies.FirstOrDefault(c => c.CompanyId == _ipAddressService.GetCompanyId())?.CompanyName ?? "comp";
            var unitName = units.FirstOrDefault(u => u.UnitId == _ipAddressService.GetUnitId())?.UnitName ?? "unit";

            var extension = Path.GetExtension(filePath);
            var newFileName = $"{companyName}-{unitName}-{customerVisitId}{extension}";
            var newPath = Path.Combine(uploadPath, newFileName);

            File.Move(filePath, newPath, overwrite: true);
            await _commandRepository.UpdateImageNameAsync(customerVisitId, newFileName, ct);
        }
    }
}
