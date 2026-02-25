using MediatR;

namespace UserManagement.Application.FinancialYear.Queries.GetFinancialYearAutoComplete
{
    public class GetFinancialYearAutoCompleteQuery : IRequest<List<GetFinancialYearAutoCompleteDto>>
    {
          public string? SearchTerm  { get; set; } 
        
    }
}