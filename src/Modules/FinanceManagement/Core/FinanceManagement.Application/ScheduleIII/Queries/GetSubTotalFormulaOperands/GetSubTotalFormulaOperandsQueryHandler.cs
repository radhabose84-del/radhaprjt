using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Queries.GetSubTotalFormulaOperands
{
    public class GetSubTotalFormulaOperandsQueryHandler
        : IRequestHandler<GetSubTotalFormulaOperandsQuery, ApiResponseDTO<List<SubTotalFormulaOperandDto>>>
    {
        private readonly IScheduleIIIQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetSubTotalFormulaOperandsQueryHandler(IScheduleIIIQueryRepository queryRepository, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<SubTotalFormulaOperandDto>>> Handle(
            GetSubTotalFormulaOperandsQuery request, CancellationToken cancellationToken)
        {
            var data = await _queryRepository.GetSubTotalFormulaOperandsAsync(request.SubTotalId);

            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "GetSubTotalFormulaOperands", actionCode: "Get", actionName: data.Count.ToString(),
                details: "Schedule III sub-total formula operands were fetched.", module: "ScheduleIIISubTotal"), cancellationToken);

            return new ApiResponseDTO<List<SubTotalFormulaOperandDto>>
            {
                IsSuccess = true, Message = "Success", Data = data, TotalCount = data.Count
            };
        }
    }
}
