using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Application.Common.HttpResponse;
using UserManagement.Application.FinancialYear.Queries.GetFinancialYear;
using MediatR;

namespace UserManagement.Application.GetFinancialYearYear.Queries.GetFinancialYear
{
    public class GetFinancialYearQuery  : IRequest<ApiResponseDTO<List<GetFinancialYearDto>>>
    {

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }

       
    }
}