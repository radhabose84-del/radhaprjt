using Contracts.Common;
using MediatR;

namespace ProductionManagement.Application.RawMaterialType.Commands.CreateRawMaterialType
{
    public class CreateRawMaterialTypeCommand : IRequest<ApiResponseDTO<int>>
    {
        public string RawMaterialTypeCode { get; set; } = string.Empty;
        public string RawMaterialTypeName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTimeOffset EffectiveFrom { get; set; }
        public DateTimeOffset? EffectiveTo { get; set; }
    }
}
