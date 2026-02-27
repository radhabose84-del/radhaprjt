using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesContact;
using SalesManagement.Application.Common.Interfaces.ISalesLead;
using SalesManagement.Domain.Events;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.SalesLead.Commands.CreateSalesLead
{
    public class CreateSalesLeadCommandHandler : IRequestHandler<CreateSalesLeadCommand, ApiResponseDTO<int>>
    {
        private readonly ISalesLeadCommandRepository _commandRepository;
        private readonly ISalesLeadQueryRepository _queryRepository;
        private readonly ISalesContactCommandRepository _salesContactCommandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateSalesLeadCommandHandler(
            ISalesLeadCommandRepository commandRepository,
            ISalesLeadQueryRepository queryRepository,
            ISalesContactCommandRepository salesContactCommandRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _salesContactCommandRepository = salesContactCommandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateSalesLeadCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.SalesLead>(request);

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

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "SALES_LEAD_CREATE",
                actionName: request.ContactName ?? request.MarketingPersonId.ToString(),
                details: $"Sales Lead created successfully with Id {newId}.",
                module: "SalesLead"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Sales Lead created successfully.",
                Data = newId
            };
        }
    }
}
