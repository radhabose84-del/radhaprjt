using UserManagement.Application.Language.Queries.GetLanguages;
using MediatR;

namespace UserManagement.Application.Language.Queries.GetLanguageAutoComplete
{
    public class GetLanguageAutoCompleteQuery : IRequest<List<LanguageAutoCompleteDTO>>
    {
        public string? SearchPattern { get; set; }
    }
}