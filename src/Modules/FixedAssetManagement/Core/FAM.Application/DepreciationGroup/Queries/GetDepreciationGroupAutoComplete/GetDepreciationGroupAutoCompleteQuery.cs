using FAM.Application.DepreciationGroup.Queries.GetDepreciationGroup;
using MediatR;

namespace FAM.Application.DepreciationGroup.Queries.GetDepreciationGroupAutoComplete
{
    public class GetDepreciationGroupAutoCompleteQuery  : IRequest<List<DepreciationGroupAutoCompleteDTO>>
    {
        public string? SearchPattern { get; set; }
    }
}