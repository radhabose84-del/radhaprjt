using AutoMapper;
using Contracts.Common;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.ICountMaster;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.CountMaster.Commands.CreateCountMaster
{
    public class CreateCountMasterCommandHandler : IRequestHandler<CreateCountMasterCommand, ApiResponseDTO<int>>
    {
        private readonly ICountMasterCommandRepository _commandRepository;
        private readonly ICountMasterQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateCountMasterCommandHandler(
            ICountMasterCommandRepository commandRepository,
            ICountMasterQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateCountMasterCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.CountMaster>(request);
            entity.CountCode = await _queryRepository.GetNextCountCodeAsync();

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "COUNT_MASTER_CREATE",
                actionName: entity.CountCode,
                details: $"Count Master '{entity.CountCode}' created successfully with Id {newId}.",
                module: "CountMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Count Master created successfully.",
                Data = newId
            };
        }
    }
}
