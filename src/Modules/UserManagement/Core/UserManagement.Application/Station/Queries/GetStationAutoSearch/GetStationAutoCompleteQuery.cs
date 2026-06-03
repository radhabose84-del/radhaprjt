using MediatR;

namespace UserManagement.Application.Station.Queries.GetStationAutoSearch
{
    public class GetStationAutoCompleteQuery : IRequest<List<StationAutoCompleteDto>>
    {
        public string? SearchPattern { get; set; }
    }
}
