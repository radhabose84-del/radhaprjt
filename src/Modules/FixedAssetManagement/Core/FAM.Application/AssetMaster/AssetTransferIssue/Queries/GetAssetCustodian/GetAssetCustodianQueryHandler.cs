using AutoMapper;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetTransferIssue;
using FluentValidation;
using MediatR;

namespace FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAssetCustodian
{
    public class GetAssetCustodianQueryHandler : IRequestHandler<GetAssetCustodianQuery, List<GetAssetCustodianDto>>
    {
        private readonly IAssetTransferQueryRepository _assetTransferQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAssetCustodianQueryHandler(IAssetTransferQueryRepository assetTransferQueryRepository, IMapper mapper, IMediator mediator)
        {
            _assetTransferQueryRepository = assetTransferQueryRepository;
            _mapper = mapper;
            _mediator = mediator;

        }

        public async Task<List<GetAssetCustodianDto>> Handle(GetAssetCustodianQuery request, CancellationToken cancellationToken)
        {
            var oldUnitId = request.OldUnitId;
              

                if (string.IsNullOrWhiteSpace(oldUnitId))
                {
                    throw new ValidationException("OldUnitId not found in token.");
                  
                }
            
              var result = await _assetTransferQueryRepository.GetCustodianByDepartmentAsync(oldUnitId, request.DepartmentId);

                return result;
        }
    }
}