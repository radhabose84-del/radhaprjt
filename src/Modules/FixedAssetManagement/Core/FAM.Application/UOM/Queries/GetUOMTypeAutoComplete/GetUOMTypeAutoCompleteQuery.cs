using MediatR;

namespace FAM.Application.UOM.Queries.GetUOMTypeAutoComplete
{
    public class GetUOMTypeAutoCompleteQuery: IRequest<List<UOMTypeAutoCompleteDto>>
    {
        public string? SearchPattern { get; set; }
    }
}