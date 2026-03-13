using AutoMapper;
using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IDocumentSequence;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.DocumentSequence.Commands.CreateDocumentSequence
{
    public class CreateDocumentSequenceCommandHandler : IRequestHandler<CreateDocumentSequenceCommand, ApiResponseDTO<int>>
    {
        private readonly IDocumentSequenceCommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateDocumentSequenceCommandHandler(
            IDocumentSequenceCommandRepository commandRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateDocumentSequenceCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.DocumentSequence>(request);

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "DOCUMENT_SEQUENCE_CREATE",
                actionName: $"Type:{request.TransactionTypeId}-Year:{request.FinancialYearId}-Doc:{request.DocNo}",
                details: $"Document Sequence created with TransactionTypeId {request.TransactionTypeId}, FinancialYearId {request.FinancialYearId}, DocNo {request.DocNo}, Id {newId}.",
                module: "DocumentSequence"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Document Sequence created successfully.",
                Data = newId
            };
        }
    }
}
