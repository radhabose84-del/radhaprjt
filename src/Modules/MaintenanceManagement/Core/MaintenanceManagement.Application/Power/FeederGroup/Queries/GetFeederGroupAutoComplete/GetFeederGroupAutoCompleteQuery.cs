using MediatR;

namespace MaintenanceManagement.Application.Power.FeederGroup.Queries.GetFeederGroupAutoComplete
{
    public class GetFeederGroupAutoCompleteQuery : IRequest<List<GetFeederGroupAutoCompleteDto>>

    {
         public string? SearchPattern { get; set; }
        
    }
}