using Contracts.Common;
using MediatR;

namespace ProductionManagement.Application.Repacking.Commands.CreateRepacking
{
    public class CreateRepackingCommand : IRequest<ApiResponseDTO<int>>
    {
        public DateOnly RepackingDate { get; set; }
        public int TotalBags { get; set; }
        public decimal NetWeight { get; set; }
        public decimal LooseConeKgs { get; set; }
        public int OldPackHeaderId { get; set; }
        public int? LooseHandlingId { get; set; }
        public string? Remarks { get; set; }
        public List<CreateRepackingDetailDto>? RepackingDetails { get; set; }
    }
}
