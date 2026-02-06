using UserManagement.Application.Common;
using UserManagement.Application.Common.HttpResponse;
using UserManagement.Application.Units.Queries.GetUnits;
using MediatR;


namespace UserManagement.Application.Units.Queries.GetUnitAutoComplete
{
    public class GetUnitAutoCompleteQuery : IRequest<List<UnitAutoCompleteDTO>>
    {
        public string? SearchPattern { get; set; }
        public int CompanyId { get; set; }        
    }
}