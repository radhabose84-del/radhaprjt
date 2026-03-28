using Contracts.Common;
using MediatR;
using ProductionManagement.Application.Repacking.Commands.CreateRepacking;

namespace ProductionManagement.Application.Repacking.Commands.UpdateRepacking
{
    public class UpdateRepackingCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public DateOnly RepackingDate { get; set; }
        public int TotalBags { get; set; }
        public decimal NetWeight { get; set; }
        public decimal LooseConeKgs { get; set; }
        public int OldPackHeaderId { get; set; }
        public int? LooseHandlingId { get; set; }
        public string? Remarks { get; set; }
        public int IsActive { get; set; }
        public List<CreateRepackingDetailDto>? RepackingDetails { get; set; }
    }
}
