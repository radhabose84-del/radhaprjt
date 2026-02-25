using MediatR;

namespace MaintenanceManagement.Application.MRS.Queries.GetCategory
{
    public class GetCategoryQuery : IRequest<List<MCategoryDto>>
    {
        public string? OldUnitcode { get; set; }   
    }
}