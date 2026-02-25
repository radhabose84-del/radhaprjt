using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.SalesGroup.Commands.UpdateSalesGroup
{
    public class UpdateSalesGroupCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public string SalesGroupName { get; set; } = null!;
        public int SalesOfficeId { get; set; }
        public string ResponsibleManager { get; set; } = null!;
        public int? ProductCategoryId { get; set; }
        public string RegionTerritory { get; set; } = null!;
        public int IsActive { get; set; }
    }
}
