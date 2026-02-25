using MediatR;

namespace UserManagement.Application.SwitchProfile.Commands.SwitchProfileByUnit
{
    public class SwitchProfileByUnitCommand : IRequest<SwitchProfileByUnitDTO>
    {
        public int UnitId { get; set; }
        public int CompanyId { get; set; }
        public int DivisionId { get; set; }
        public string? OldUnitId { get; set; }
    }
}