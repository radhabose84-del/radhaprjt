using Contracts.Common;
using FinanceManagement.Application.ScheduleIII.Dto;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Queries.GetLinesAutoComplete
{
    // Autocomplete of the token structure's included lines (section + line item), ordered by DisplayOrder.
    public class GetLinesAutoCompleteQuery : IRequest<ApiResponseDTO<List<ScheduleIIILineLookupDto>>>
    {
        public string? Term { get; set; }   // optional search on line code / line name / section name
    }
}
