// GetById
namespace InventoryManagement.Application.Item.Templates.Queries.GetInspectionTemplateById
{
    using InventoryManagement.Application.Item.Templates.DTOs;
    using MediatR;
    public sealed record GetInspectionTemplateByIdQuery : IRequest<InspectionTemplateDto?>
    {
        public int Id { get; init; }
    }
}
