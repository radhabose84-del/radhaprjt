using Contracts.Common;
using MediatR;

namespace GateEntryManagement.Application.MiscTypeMaster.Commands.CreateMiscTypeMaster
{
    public class CreateMiscTypeMasterCommand : IRequest<ApiResponseDTO<int>>
    {
        public string? MiscTypeCode { get; set; }
        public string? Description { get; set; }
    }
}
