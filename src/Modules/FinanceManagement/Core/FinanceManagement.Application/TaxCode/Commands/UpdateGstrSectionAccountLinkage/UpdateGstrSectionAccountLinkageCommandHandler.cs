using AutoMapper;
using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IGstrSection;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Commands.UpdateGstrSectionAccountLinkage
{
    public class UpdateGstrSectionAccountLinkageCommandHandler : IRequestHandler<UpdateGstrSectionAccountLinkageCommand, ApiResponseDTO<int>>
    {
        private readonly IGstrSectionCommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateGstrSectionAccountLinkageCommandHandler(
            IGstrSectionCommandRepository commandRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateGstrSectionAccountLinkageCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.GstrSectionAccountLinkage>(request);

            var result = await _commandRepository.UpdateLinkageAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "GSTR_SECTION_LINKAGE_UPDATE",
                actionName: request.Id.ToString(),
                details: $"GSTR section-account mapping with Id {request.Id} updated successfully.",
                module: "GstrSectionAccountLinkage"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "GSTR section-account mapping updated successfully.",
                Data = result
            };
        }
    }
}
