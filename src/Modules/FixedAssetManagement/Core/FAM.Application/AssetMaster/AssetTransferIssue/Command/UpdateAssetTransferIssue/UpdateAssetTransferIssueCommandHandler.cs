using AutoMapper;
using FAM.Application.Common.Interfaces;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetTransferIssue;
using FluentValidation;
using MediatR;

namespace FAM.Application.AssetMaster.AssetTransferIssue.Command.UpdateAssetTransferIssue
{
    public class UpdateAssetTransferIssueCommandHandler  : IRequestHandler<UpdateAssetTransferIssueCommand, bool>
    {

        private readonly IAssetTransferCommandRepository _assetTransferCommandRepository;
        private readonly IAssetTransferQueryRepository _assetTransferQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IIPAddressService _ipAddressService;
        private readonly ITimeZoneService _timeZoneService;
        private readonly IValidator<UpdateAssetTransferIssueCommand> _validator;

        public UpdateAssetTransferIssueCommandHandler(
            IAssetTransferCommandRepository assetTransferCommandRepository,
            IAssetTransferQueryRepository assetTransferQueryRepository,
            IMapper mapper,
            IMediator mediator,
            IIPAddressService ipAddressService,
            ITimeZoneService timeZoneService,
            IValidator<UpdateAssetTransferIssueCommand> validator)
        {
            _assetTransferCommandRepository = assetTransferCommandRepository;
            _assetTransferQueryRepository = assetTransferQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            _ipAddressService = ipAddressService;
            _timeZoneService = timeZoneService;
            _validator = validator;
        }
            public async Task<bool> Handle(UpdateAssetTransferIssueCommand request, CancellationToken cancellationToken)
        {
                            // 🔹 Retrieve Existing Record from Query Repository
                var existingRecordDto = await _assetTransferQueryRepository.GetAssetTransferByIdAsync(request.AssetTransferHdr.Id);
                if (existingRecordDto == null)
                {
                    throw new ValidationException($"Asset Transfer Issue with ID {request.AssetTransferHdr.Id} not found.");
                }

                // 🔹 Convert DTO to Domain Entity
                var assetTransferIssueHdr = _mapper.Map<FAM.Domain.Entities.AssetMaster.AssetTransferIssueHdr>(request.AssetTransferHdr);                
                assetTransferIssueHdr.ModifiedDate = _timeZoneService.GetCurrentTime(_timeZoneService.GetSystemTimeZone());
                assetTransferIssueHdr.ModifiedBy = _ipAddressService.GetUserId();
                assetTransferIssueHdr.ModifiedByName = _ipAddressService.GetUserName();
                assetTransferIssueHdr.ModifiedIP = _ipAddressService.GetSystemIPAddress();

                
                // 🔹 Save Changes
                var result = await _assetTransferCommandRepository.UpdateAssetTransferAsync(assetTransferIssueHdr);

                if (result)
                {
                    return result;
                }
                throw new Exception("Asset Transfer update failed");
                     
        }

    }
}