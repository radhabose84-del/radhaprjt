using AutoMapper;
using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Commands.CreateGstrSectionMapping
{
    public class CreateGstrSectionMappingCommandHandler : IRequestHandler<CreateGstrSectionMappingCommand, ApiResponseDTO<int>>
    {
        private readonly ITaxCodeCommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateGstrSectionMappingCommandHandler(
            ITaxCodeCommandRepository commandRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateGstrSectionMappingCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.GstrSectionMapping>(request);

            var newId = await _commandRepository.CreateGstrMappingAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "GSTR_SECTION_MAPPING_CREATE",
                actionName: request.SectionCode ?? string.Empty,
                details: $"GSTR section mapping '{request.SectionCode}' created successfully with Id {newId}.",
                module: "GstrSectionMapping"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "GSTR section mapping created successfully.",
                Data = newId
            };
        }
    }
}
