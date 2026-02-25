using MediatR;

namespace FAM.Application.Location.Command.UpdateSubLocation
{
    public class UpdateSubLocationCommand: IRequest<bool>
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string? SubLocationName { get; set; }
        public string? Description { get; set; }
        public int UnitId { get; set; }
        public int DepartmentId { get; set; }
        public int LocationId { get; set; }
        public byte IsActive { get; set; }
    }
}