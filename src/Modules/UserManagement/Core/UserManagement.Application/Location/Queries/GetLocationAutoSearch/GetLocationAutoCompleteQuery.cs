using MediatR;

namespace UserManagement.Application.Location.Queries.GetLocationAutoSearch
{
    public class GetLocationAutoCompleteQuery : IRequest<List<LocationAutoCompleteDto>>
    {
        public string? SearchPattern { get; set; }
    }
}
