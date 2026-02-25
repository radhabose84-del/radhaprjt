using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.SalesGroup.Commands.CreateSalesGroup
{
    public class CreateSalesGroupCommand : IRequest<ApiResponseDTO<int>>
    {
        public string SalesGroupName { get; set; } = null!;
        public int SalesOfficeId { get; set; }
        public string ResponsibleManager { get; set; } = null!;
        public int? ProductCategoryId { get; set; }
        public string RegionTerritory { get; set; } = null!;
    }
}
