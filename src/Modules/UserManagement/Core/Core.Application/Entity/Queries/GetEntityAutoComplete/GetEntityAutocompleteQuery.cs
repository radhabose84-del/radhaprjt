using Core.Application.Common;
using Core.Application.Common.HttpResponse;
using Core.Application.Entity.Queries.GetEntity;
using MediatR;

namespace Core.Application.Entity.Queries.GetEntityAutoComplete
{
    public class GetEntityAutocompleteQuery : IRequest<List<EntityAutoCompleteDto>>
    {
        public string? SearchPattern { get; set; }
    }
}