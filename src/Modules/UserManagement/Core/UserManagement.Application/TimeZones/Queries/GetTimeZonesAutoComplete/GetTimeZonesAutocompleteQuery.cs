using UserManagement.Application.TimeZones.Queries.GetTimeZones;
using MediatR;

namespace UserManagement.Application.TimeZones.Queries.GetTimeZonesAutoComplete
{
    public class GetTimeZonesAutocompleteQuery : IRequest<List<TimeZonesAutoCompleteDto>>
    {
         public string? SearchPattern { get; set; }
    }
}