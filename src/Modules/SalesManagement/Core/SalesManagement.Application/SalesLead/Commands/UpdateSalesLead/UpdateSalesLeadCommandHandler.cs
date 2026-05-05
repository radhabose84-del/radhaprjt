using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.Common.Interfaces.ISalesContact;
using SalesManagement.Application.Common.Interfaces.ISalesLead;
using SalesManagement.Domain.Events;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.SalesLead.Commands.UpdateSalesLead
{
    public class UpdateSalesLeadCommandHandler : IRequestHandler<UpdateSalesLeadCommand, ApiResponseDTO<int>>
    {
        private readonly ISalesLeadCommandRepository _commandRepository;
        private readonly ISalesLeadQueryRepository _queryRepository;
        private readonly ISalesContactCommandRepository _salesContactCommandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IMarketingOfficerAccessFilter _accessFilter;

        public UpdateSalesLeadCommandHandler(
            ISalesLeadCommandRepository commandRepository,
            ISalesLeadQueryRepository queryRepository,
            ISalesContactCommandRepository salesContactCommandRepository,
            IMediator mediator,
            IMapper mapper,
            IMarketingOfficerAccessFilter accessFilter)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _salesContactCommandRepository = salesContactCommandRepository;
            _mediator = mediator;
            _mapper = mapper;
            _accessFilter = accessFilter;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateSalesLeadCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.SalesLead>(request);

            // Auto-set MarketingOfficerId from token when user is a marketing officer
            if (await _accessFilter.ShouldApplyFilterAsync(cancellationToken))
            {
                var officerId = _accessFilter.GetCurrentMarketingOfficerId();
                if (officerId.HasValue)
                    entity.MarketingOfficerId = officerId.Value;
            }

            // Auto-create SalesContact when ContactId is null and ContactName is provided
            if (!request.ContactId.HasValue && !string.IsNullOrWhiteSpace(request.ContactName))
            {
                var primaryContactTypeId = await _queryRepository.GetPrimaryContactTypeIdAsync();
                var newContact = new Domain.Entities.SalesContact
                {
                    ContactName = request.ContactName,
                    MobileNumber = request.MobileNumber,
                    Email = request.EmailId,
                    PartyId = request.PartyId,
                    ContactTypeId = primaryContactTypeId,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                var contactId = await _salesContactCommandRepository.CreateAsync(newContact);
                entity.ContactId = contactId;
            }

            var updatedId = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "SALES_LEAD_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Sales Lead with Id {request.Id} updated successfully.",
                module: "SalesLead"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Sales Lead updated successfully.",
                Data = updatedId
            };
        }
    }
}
