using FAM.Application.Location.Queries.GetSubLocations;
using MediatR;

namespace FAM.Application.SubLocation.Queries.GetSubLocationAutoComplete
{
    public class GetSubLocationAutoCompleteQuery :  IRequest<List<SubLocationAutoCompleteDto>>
    {
        public string? SearchPattern { get; set; }
        
    }
}