using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.ProfitCentre.Commands.CreateProfitCentre
{
    public class CreateProfitCentreCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public string? ProfitCentreCode { get; set; }   // unique across all companies, immutable
        public string? ProfitCentreName { get; set; }

        public int LevelId { get; set; }                 // Finance.MiscMaster (L1 Segment / L2 Sub-segment)
        public int? ParentProfitCentreId { get; set; }   // null for L1; an L1 Segment for L2

        // Optional (non-mandatory) — resolved via IUserLookup; saved null until the FE wires the picker.
        public int? ResponsibleHeadId { get; set; }

        // Revenue segments are revenue-linked by default.
        public bool IsRevenueLinked { get; set; } = true;

        // AC#4 — supplied when a PC is added mid-year; an audit note records that prior transactions
        // cannot be retro-tagged.
        public string? MidYearJustification { get; set; }

        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
