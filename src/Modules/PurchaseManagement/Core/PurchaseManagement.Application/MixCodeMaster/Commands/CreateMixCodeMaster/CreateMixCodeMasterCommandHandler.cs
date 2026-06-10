using AutoMapper;
using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IMixCodeMaster;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.MixCodeMaster.Commands.CreateMixCodeMaster
{
    public class CreateMixCodeMasterCommandHandler : IRequestHandler<CreateMixCodeMasterCommand, ApiResponseDTO<int>>
    {
        private readonly IMixCodeMasterCommandRepository _commandRepository;
        private readonly IMixCodeMasterQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateMixCodeMasterCommandHandler(
            IMixCodeMasterCommandRepository commandRepository,
            IMixCodeMasterQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateMixCodeMasterCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.MixCodeMaster>(request);

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "MIXCODEMASTER_CREATE",
                actionName: request.MixCode,
                details: $"MixCodeMaster '{request.MixCode}' created successfully with Id {newId}.",
                module: "MixCodeMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Mix Code created successfully.",
                Data = newId
            };
        }
    }
}
