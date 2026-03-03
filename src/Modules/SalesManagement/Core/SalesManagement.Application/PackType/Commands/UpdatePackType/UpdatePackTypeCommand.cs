using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.PackType.Commands.UpdatePackType
{
    public class UpdatePackTypeCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public string? PackTypeName { get; set; }
        public decimal NetWeight { get; set; }
        public decimal TareWeight { get; set; }
        public int? ConesPerBag { get; set; }
        public bool ProductionAllowed { get; set; }
        public int IsActive { get; set; }
    }
}
