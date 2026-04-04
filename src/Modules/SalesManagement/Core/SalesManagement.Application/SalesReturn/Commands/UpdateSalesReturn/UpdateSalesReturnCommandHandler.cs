using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesReturn;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Events;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.SalesReturn.Commands.UpdateSalesReturn
{
    public class UpdateSalesReturnCommandHandler : IRequestHandler<UpdateSalesReturnCommand, ApiResponseDTO<int>>
    {
        private readonly ISalesReturnCommandRepository _commandRepository;
        private readonly ISalesReturnQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateSalesReturnCommandHandler(
            ISalesReturnCommandRepository commandRepository,
            ISalesReturnQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateSalesReturnCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<SalesReturnHeader>(request);
            entity.IsActive = request.IsActive == 1 ? Status.Active : Status.Inactive;

            var details = new List<SalesReturnDetail>();
            if (request.Details != null && request.Details.Count > 0)
            {
                foreach (var detail in request.Details)
                {
                    var detailEntity = _mapper.Map<SalesReturnDetail>(detail);
                    details.Add(detailEntity);
                }
            }

            var result = await _commandRepository.UpdateAsync(entity, details);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "SALES_RETURN_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Sales Return with Id {request.Id} updated successfully.",
                module: "SalesReturn");
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Sales Return updated successfully.",
                Data = result
            };
        }
    }
}
