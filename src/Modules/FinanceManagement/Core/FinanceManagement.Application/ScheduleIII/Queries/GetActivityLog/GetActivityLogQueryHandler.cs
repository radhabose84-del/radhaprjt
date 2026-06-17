using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Dto;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Queries.GetActivityLog
{
    public class GetActivityLogQueryHandler : IRequestHandler<GetActivityLogQuery, ApiResponseDTO<List<ActivityLogDto>>>
    {
        private readonly IScheduleIIIQueryRepository _queryRepository;

        public GetActivityLogQueryHandler(IScheduleIIIQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<ApiResponseDTO<List<ActivityLogDto>>> Handle(GetActivityLogQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetActivityLogAsync(
                request.EntityName, request.EntityId, request.PageNumber, request.PageSize);

            return new ApiResponseDTO<List<ActivityLogDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = data,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
