using MediatR;

namespace UserManagement.Application.Divisions.Commands.DeleteDivision
{
    public class DeleteDivisionCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }
}