using MediatR;
using UserManagement.Application.FinancialYear.Queries.GetFinancialYear;

namespace UserManagement.Application.FinancialYear.Command.CreateFinancialYear
{
    public class CreateFinancialYearCommand : IRequest<FinancialYearDto>
    {

        public int Id { get; set; }
        public string? StartYear { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; } 
        public string? FinYearName { get; set; }
    }
}