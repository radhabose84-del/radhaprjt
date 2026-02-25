#nullable disable
using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOffice;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesOffice.Commands.UpdateSalesOffice
{
    public class UpdateSalesOfficeCommandHandler : IRequestHandler<UpdateSalesOfficeCommand, ApiResponseDTO<int>>
    {
        private readonly ISalesOfficeCommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateSalesOfficeCommandHandler(
            ISalesOfficeCommandRepository commandRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateSalesOfficeCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.SalesOffice>(request);

            var updatedId = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "SALES_OFFICE_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Sales Office with Id {request.Id} updated successfully.",
                module: "SalesOffice"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Sales Office updated successfully.",
                Data = updatedId
            };
        }
    }
}
