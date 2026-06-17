using AutoMapper;
using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IGstrSection;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Commands.CreateGstrSectionAccountLinkage
{
    public class CreateGstrSectionAccountLinkageCommandHandler : IRequestHandler<CreateGstrSectionAccountLinkageCommand, ApiResponseDTO<int>>
    {
        private readonly IGstrSectionCommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateGstrSectionAccountLinkageCommandHandler(
            IGstrSectionCommandRepository commandRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateGstrSectionAccountLinkageCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.GstrSectionAccountLinkage>(request);

            var newId = await _commandRepository.CreateLinkageAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "GSTR_SECTION_LINKAGE_CREATE",
                actionName: newId.ToString(),
                details: $"GSTR section-account mapping created (SectionMasterId {request.SectionMasterId}) with Id {newId}.",
                module: "GstrSectionAccountLinkage"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "GSTR section-account mapping created successfully.",
                Data = newId
            };
        }
    }
}
