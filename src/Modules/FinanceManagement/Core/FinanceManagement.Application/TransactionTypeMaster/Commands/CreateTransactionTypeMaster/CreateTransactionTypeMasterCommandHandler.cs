using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.ITransactionTypeMaster;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.TransactionTypeMaster.Commands.CreateTransactionTypeMaster
{
    public class CreateTransactionTypeMasterCommandHandler : IRequestHandler<CreateTransactionTypeMasterCommand, ApiResponseDTO<int>>
    {
        private readonly ITransactionTypeMasterCommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IIPAddressService _ipAddressService;

        public CreateTransactionTypeMasterCommandHandler(
            ITransactionTypeMasterCommandRepository commandRepository,
            IMediator mediator,
            IMapper mapper,
            IIPAddressService ipAddressService)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _mapper = mapper;
            _ipAddressService = ipAddressService;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateTransactionTypeMasterCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.TransactionTypeMaster>(request);
            entity.UnitId = _ipAddressService.GetUnitId() ?? 0;

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
