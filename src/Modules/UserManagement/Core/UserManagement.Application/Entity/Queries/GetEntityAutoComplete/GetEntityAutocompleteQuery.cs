using UserManagement.Application.Common;
using UserManagement.Application.Common.HttpResponse;
using UserManagement.Application.Entity.Queries.GetEntity;
using MediatR;

namespace UserManagement.Application.Entity.Queries.GetEntityAutoComplete
{
    public class GetEntityAutocompleteQuery : IRequest<List<EntityAutoCompleteDto>>
    {
        public string? SearchPattern { get; set; }
    }
}