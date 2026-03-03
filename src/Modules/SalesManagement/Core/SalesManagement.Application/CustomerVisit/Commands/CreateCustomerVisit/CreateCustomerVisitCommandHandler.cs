using AutoMapper;
using Contracts.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using SalesManagement.Application.Common.Interfaces.ICustomerVisit;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.CustomerVisit.Commands.CreateCustomerVisit
{
    public class CreateCustomerVisitCommandHandler : IRequestHandler<CreateCustomerVisitCommand, ApiResponseDTO<int>>
    {
        private readonly ICustomerVisitCommandRepository _commandRepository;
        private readonly ICustomerVisitQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateCustomerVisitCommandHandler> _logger;

        public CreateCustomerVisitCommandHandler(
            ICustomerVisitCommandRepository commandRepository,
            ICustomerVisitQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper,
            ILogger<CreateCustomerVisitCommandHandler> logger)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateCustomerVisitCommand request, CancellationToken cancellationToken)
        {
            // Pre-check: if image is provided, verify file exists on disk
            string? uploadPath = null;
            if (!string.IsNullOrWhiteSpace(request.ImageName))
            {
                uploadPath = BuildImageUploadPath();
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

            var newId = await _commandRepository.CreateAsync(entity);

            // Post-save: rename image to {marketingOfficerName}-{customerVisitId}.ext
            if (!string.IsNullOrWhiteSpace(request.ImageName) && uploadPath != null)
            {
                try
                {
                    await RenameCustomerVisitImageAsync(newId, request.MarketingOfficerId, request.ImageName!, uploadPath, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to rename customer visit image for Id {CustomerVisitId}.", newId);
                }
            }

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "CUSTOMER_VISIT_CREATE",
                actionName: newId.ToString(),
                details: $"CustomerVisit created successfully with Id {newId}.",
                module: "CustomerVisit"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Customer Visit created successfully.",
                Data = newId
            };
        }

        private static string BuildImageUploadPath()
        {
            return Path.Combine(
                Directory.GetCurrentDirectory(),
                "Resources",
                MiscEnumEntity.CustomerVisit);
        }

        private async Task RenameCustomerVisitImageAsync(int customerVisitId, int marketingOfficerId, string originalFileName, string uploadPath, CancellationToken ct)
        {
            var filePath = Path.Combine(uploadPath, originalFileName);
            if (!File.Exists(filePath))
                return;

            var moName = await _queryRepository.GetMarketingOfficerNameAsync(marketingOfficerId) ?? "officer";

            var extension = Path.GetExtension(filePath);
            var newFileName = $"{moName}-{customerVisitId}{extension}";
            var newPath = Path.Combine(uploadPath, newFileName);

            File.Move(filePath, newPath, overwrite: true);
            await _commandRepository.UpdateImageNameAsync(customerVisitId, newFileName, ct);
        }
    }
}
