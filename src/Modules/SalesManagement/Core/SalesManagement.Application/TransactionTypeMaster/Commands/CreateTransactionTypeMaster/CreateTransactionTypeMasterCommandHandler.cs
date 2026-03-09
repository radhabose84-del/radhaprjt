using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ITransactionTypeMaster;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.TransactionTypeMaster.Commands.CreateTransactionTypeMaster
{
    public class CreateTransactionTypeMasterCommandHandler : IRequestHandler<CreateTransactionTypeMasterCommand, ApiResponseDTO<int>>
    {
        private readonly ITransactionTypeMasterCommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateTransactionTypeMasterCommandHandler(
            ITransactionTypeMasterCommandRepository commandRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateTransactionTypeMasterCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.TransactionTypeMaster>(request);

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "TRANSACTION_TYPE_CREATE",
                actionName: request.TypeName ?? string.Empty,
                details: $"Transaction Type Master '{request.TypeName}' created successfully with Id {newId}.",
                module: "TransactionTypeMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Transaction Type Master created successfully.",
                Data = newId
            };
        }
    }
}
