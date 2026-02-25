using MediatR;

namespace MaintenanceManagement.Application.MRS.Command.CreateMRS
{
    public class CreateMRSCommand :IRequest<int>
    {
         public HeaderRequest? Header { get; set; }
    }
}