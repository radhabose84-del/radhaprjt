using FAM.Application.Location.Queries.GetLocations;
using MediatR;

namespace FAM.Application.Location.Queries.GetLocationAutoComplete
{
    public class GetLocationAutoCompleteQuery : IRequest<List<LocationAutoCompleteDto>>
    {
        public string? SearchPattern { get; set; }
    }
}