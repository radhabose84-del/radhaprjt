using MediatR;

namespace MaintenanceManagement.Application.Power.Feeder.Queries.GetFeederAutoComplete
{
    public class GetFeederAutoCompleteQuery : IRequest<List<GetFeederAutoCompleteDto>>  
    {
            public string? SearchPattern { get; set; }
    }
}