using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesAgreement;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesAgreement.Commands.UpdateSalesAgreement
{
    public class UpdateSalesAgreementCommandHandler : IRequestHandler<UpdateSalesAgreementCommand, ApiResponseDTO<int>>
    {
        private readonly ISalesAgreementCommandRepository _commandRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public UpdateSalesAgreementCommandHandler(
            ISalesAgreementCommandRepository commandRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateSalesAgreementCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<SalesAgreementHeader>(request);

            var result = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "SALESAGREEMENT_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Sales Agreement with Id {request.Id} updated successfully.",
                module: "SalesAgreement");
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Sales Agreement updated successfully.",
                Data = result
            };
        }
    }
}
