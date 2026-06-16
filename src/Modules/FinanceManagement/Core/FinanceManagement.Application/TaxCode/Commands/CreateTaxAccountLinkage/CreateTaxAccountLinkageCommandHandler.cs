using AutoMapper;
using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Commands.CreateTaxAccountLinkage
{
    public class CreateTaxAccountLinkageCommandHandler : IRequestHandler<CreateTaxAccountLinkageCommand, ApiResponseDTO<int>>
    {
        private readonly ITaxCodeCommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateTaxAccountLinkageCommandHandler(
            ITaxCodeCommandRepository commandRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateTaxAccountLinkageCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.TaxAccountLinkage>(request);

            var newId = await _commandRepository.CreateLinkageAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "TAX_ACCOUNT_LINKAGE_CREATE",
                actionName: newId.ToString(),
                details: $"Tax-account linkage created (TaxCodeId {request.TaxCodeId} -> GlAccountId {request.GlAccountId}) with Id {newId}.",
                module: "TaxAccountLinkage"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Tax-account linkage created successfully.",
                Data = newId
            };
        }
    }
}
