using PurchaseManagement.Application.Common.Interfaces.ITnCTemplateMaster;
using MediatR;

namespace PurchaseManagement.Application.TnCTemplateMaster.Queries.GetTnCTemplateMasterAutoComplete
{
    public class TnCTemplateAutoCompleteQueryHandler : IRequestHandler<TnCTemplateAutoCompleteQuery, List<TnCAutoCompleteDto>>
    {

        private readonly ITnCTemplateMasterQueryRepository _tnCTemplateMasterQueryRepository;
        
        public TnCTemplateAutoCompleteQueryHandler( ITnCTemplateMasterQueryRepository tnCTemplateMasterQueryRepository)
        {
            _tnCTemplateMasterQueryRepository = tnCTemplateMasterQueryRepository;
        }

        public async Task<List<TnCAutoCompleteDto>> Handle(TnCTemplateAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var search = request.SearchPattern?.Trim();
           

            var items = await _tnCTemplateMasterQueryRepository.GetTnCTemplateAutoCompleteAsync(
                search,
                
                request.TemplateTypeId,
                request.ApplicabilityId
                );

            return items  ;
        }
    }
}