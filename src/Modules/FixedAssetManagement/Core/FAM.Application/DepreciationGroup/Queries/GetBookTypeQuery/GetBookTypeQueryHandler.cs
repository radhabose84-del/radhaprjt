using AutoMapper;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.IDepreciationGroup;
using FAM.Application.MiscMaster.Queries.GetMiscMaster;
using MediatR;

namespace FAM.Application.DepreciationGroup.Queries.GetBookTypeQuery
{
    public class GetBookTypeQueryHandler : IRequestHandler<GetBookTypeQuery, List<GetMiscMasterDto>>
    {
        private readonly IDepreciationGroupQueryRepository _repository;
        private readonly IMapper _mapper;

        public GetBookTypeQueryHandler(IDepreciationGroupQueryRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<GetMiscMasterDto>> Handle(GetBookTypeQuery request, CancellationToken cancellationToken)
        {
            var bookTypes = await _repository.GetBookTypeAsync();
            var bookTypeDtoList = _mapper.Map<List<GetMiscMasterDto>>(bookTypes);           
            return bookTypeDtoList;
        }
    }
}