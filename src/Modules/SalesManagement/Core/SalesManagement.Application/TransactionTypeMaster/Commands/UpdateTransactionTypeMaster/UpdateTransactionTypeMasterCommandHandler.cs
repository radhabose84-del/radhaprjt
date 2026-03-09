using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ITransactionTypeMaster;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.TransactionTypeMaster.Commands.UpdateTransactionTypeMaster
{
    public class UpdateTransactionTypeMasterCommandHandler : IRequestHandler<UpdateTransactionTypeMasterCommand, ApiResponseDTO<int>>
    {
        private readonly ITransactionTypeMasterCommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateTransactionTypeMasterCommandHandler(
            ITransactionTypeMasterCommandRepository commandRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateTransactionTypeMasterCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.TransactionTypeMaster>(request);

            var result = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "TRANSACTION_TYPE_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Transaction Type Master with Id {request.Id} updated successfully.",
                module: "TransactionTypeMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Transaction Type Master updated successfully.",
                Data = result
            };
        }
    }
}
