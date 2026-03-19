using Contracts.Common;
using MediatR;

namespace ProductionManagement.Application.PackType.Commands.CreatePackType
{
    public class CreatePackTypeCommand : IRequest<ApiResponseDTO<int>>
    {
        public string? PackTypeCode { get; set; }
        public string? PackTypeName { get; set; }
        public decimal NetWeight { get; set; }
        public decimal TareWeight { get; set; }
        public int? ConesPerBag { get; set; }
        public bool ProductionAllowed { get; set; } = true;
    }
}
