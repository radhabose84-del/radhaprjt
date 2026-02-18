using UserManagement.Application.Common;
using Contracts.Common;
using UserManagement.Application.Entity.Queries.GetEntity;
using MediatR;

namespace UserManagement.Application.Entity.Queries.GetEntityAutoComplete
{
    public class GetEntityAutocompleteQuery : IRequest<List<EntityAutoCompleteDto>>
    {
        public string? SearchPattern { get; set; }
    }
}