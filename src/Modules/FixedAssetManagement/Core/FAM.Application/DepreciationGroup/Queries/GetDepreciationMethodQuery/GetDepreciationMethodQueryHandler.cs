using AutoMapper;
using FAM.Application.Common.Interfaces.IDepreciationGroup;
using FAM.Application.MiscMaster.Queries.GetMiscMaster;
using MediatR;

namespace FAM.Application.DepreciationGroup.Queries.GetDepreciationMethodQuery
{
    public class GetDepreciationMethodQueryHandler : IRequestHandler<GetDepreciationMethodQuery, List<GetMiscMasterDto>>
    {
        private readonly IDepreciationGroupQueryRepository _repository;
        private readonly IMapper _mapper;

        public GetDepreciationMethodQueryHandler(IDepreciationGroupQueryRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<GetMiscMasterDto>> Handle(GetDepreciationMethodQuery request, CancellationToken cancellationToken)
        {
            var DepMethod = await _repository.GetDepreciationMethodAsync();
            var DepMethodDtoList = _mapper.Map<List<GetMiscMasterDto>>(DepMethod);
            return DepMethodDtoList;
        }
    }
}