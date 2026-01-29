using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.HttpResponse;
using Core.Application.TimeZones.Queries.GetTimeZones;
using MediatR;

namespace Core.Application.TimeZones.Queries.GetTimeZonesAutoComplete
{
    public class GetTimeZonesAutocompleteQuery : IRequest<List<TimeZonesAutoCompleteDto>>
    {
         public string? SearchPattern { get; set; }
    }
}