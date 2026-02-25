using MediatR;

namespace UserManagement.Application.FinancialYear.Command.DeleteFinancialYear
{
    public class DeleteFinancialYearCommand :IRequest<int>
    {

          public int Id { get; set; }
        
    }
}