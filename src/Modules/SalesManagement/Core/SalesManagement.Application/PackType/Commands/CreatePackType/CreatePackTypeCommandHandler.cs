using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IPackType;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.PackType.Commands.CreatePackType
{
    public class CreatePackTypeCommandHandler : IRequestHandler<CreatePackTypeCommand, ApiResponseDTO<int>>
    {
        private readonly IPackTypeCommandRepository _commandRepository;
        private readonly IPackTypeQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreatePackTypeCommandHandler(
            IPackTypeCommandRepository commandRepository,
            IPackTypeQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreatePackTypeCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.PackType>(request);

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "PACKTYPE_CREATE",
                actionName: request.PackTypeCode ?? string.Empty,
                details: $"PackType '{request.PackTypeCode}' created successfully with Id {newId}.",
                module: "PackType"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "PackType created successfully.",
                Data = newId
            };
        }
    }
}
