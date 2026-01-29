using Core.Application.Common;
using Core.Application.Common.HttpResponse;
using Core.Application.Units.Queries.GetUnits;
using MediatR;


namespace Core.Application.Units.Queries.GetUnitAutoComplete
{
    public class GetUnitAutoCompleteQuery : IRequest<List<UnitAutoCompleteDTO>>
    {
        public string? SearchPattern { get; set; }
        public int CompanyId { get; set; }        
    }
}