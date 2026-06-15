using UserManagement.Application.IconMaster.Queries.GetIconMaster;
using MediatR;

namespace UserManagement.Application.IconMaster.Queries.GetIconMasterAutoComplete
{
    public class GetIconMasterAutocompleteQuery : IRequest<List<IconMasterAutoCompleteDto>>
    {
        public string SearchPattern { get; set; } = default!;
    }
}
