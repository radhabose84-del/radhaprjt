using Contracts.Common;
using UserManagement.Application.Common.Mappings;


namespace UserManagement.Application.FinancialYear.Queries.GetFinancialYear
{
    public class FinancialYearDto  : IMapFrom<ApiResponseDTO<UserManagement.Domain.Entities.FinancialYear>>
    {
         public int Id { get; set; }
        public string? StartYear { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; } 

        public string? FinYearName { get; set; }


        


    }
}