using AutoMapper;
using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Commands.UpdateGstrSectionMapping
{
    public class UpdateGstrSectionMappingCommandHandler : IRequestHandler<UpdateGstrSectionMappingCommand, ApiResponseDTO<int>>
    {
        private readonly ITaxCodeCommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateGstrSectionMappingCommandHandler(
            ITaxCodeCommandRepository commandRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateGstrSectionMappingCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.GstrSectionMapping>(request);

            var updatedId = await _commandRepository.UpdateGstrMappingAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "GSTR_SECTION_MAPPING_UPDATE",
                actionName: request.Id.ToString(),
                details: $"GSTR section mapping with Id {request.Id} updated successfully.",
                module: "GstrSectionMapping"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "GSTR section mapping updated successfully.",
                Data = updatedId
            };
        }
    }
}
