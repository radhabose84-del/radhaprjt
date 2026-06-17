using AutoMapper;
using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IGstrSection;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Commands.UpdateGstrSectionMaster
{
    public class UpdateGstrSectionMasterCommandHandler : IRequestHandler<UpdateGstrSectionMasterCommand, ApiResponseDTO<int>>
    {
        private readonly IGstrSectionCommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateGstrSectionMasterCommandHandler(
            IGstrSectionCommandRepository commandRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateGstrSectionMasterCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.GstrSectionMaster>(request);

            var result = await _commandRepository.UpdateSectionAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "GSTR_SECTION_MASTER_UPDATE",
                actionName: request.Id.ToString(),
                details: $"GSTR section with Id {request.Id} updated successfully.",
                module: "GstrSectionMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "GSTR section updated successfully.",
                Data = result
            };
        }
    }
}
