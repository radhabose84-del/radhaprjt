using MediatR;

namespace MaintenanceManagement.Application.Power.GeneratorConsumption.Queries.GetClosingEnergyReaderValueById
{
    public class GetClosingEnergyReaderValueByIdQuery :  IRequest<GetClosingEnergyReaderValueDto>
    {
        public int GeneratorId { get; set; }
    }
}