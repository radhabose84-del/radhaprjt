#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using FAM.Application.Common.Interfaces;
using FAM.Application.Common.Interfaces.IAssetTransferReceipt;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.AssetMaster.AssetTransferReceipt.Command.CreateAssetTransferReceipt
{
    public class CreateAssetTransferReceiptCommandHandler : IRequestHandler<CreateAssetTransferReceiptCommand, int>
    {
        private readonly IAssetTransferReceiptCommandRepository _iassettransferreceiptcommandrepository;
        private readonly IAssetTransferReceiptQueryRepository _iassettransferreceiptqueryrepository;
        private readonly IMediator _imediator;
        private readonly IMapper _imapper;
        private readonly IIPAddressService _ipAddressService;
        private readonly ITimeZoneService _timeZoneService;

        
        public CreateAssetTransferReceiptCommandHandler(IAssetTransferReceiptCommandRepository assetTransferReceiptCommandRepository, IMediator imediator, IMapper imapper, IIPAddressService ipAddressService, ITimeZoneService timeZoneService,IAssetTransferReceiptQueryRepository iassettransferreceiptqueryrepository)
        {
            _iassettransferreceiptcommandrepository=assetTransferReceiptCommandRepository;
            _imediator=imediator;
            _imapper=imapper;
            _ipAddressService=ipAddressService;
            _timeZoneService=timeZoneService;
            _iassettransferreceiptqueryrepository=iassettransferreceiptqueryrepository;

        }
        public async Task<int> Handle(CreateAssetTransferReceiptCommand request, CancellationToken cancellationToken)
        {

            //Asset Location Mapping
            var ToLocationUpdate = await _iassettransferreceiptqueryrepository
                .GetByAssetTransferId(request.AssetTransferReceiptHdrDto.AssetTransferId);

              var ToCustodianId = ToLocationUpdate.ToCustodianId;
              var ToUnitId = ToLocationUpdate.ToUnitId;
              var ToDepartmentId = ToLocationUpdate.ToDepartmentId;

                     var assetLocation = request.AssetTransferReceiptHdrDto.AssetTransferReceiptDtl.Select(dto => {
                         var entity = _imapper.Map<FAM.Domain.Entities.AssetMaster.AssetLocation>(dto);
                         entity.CustodianId = ToCustodianId;
                         entity.UnitId = ToUnitId;
                         entity.DepartmentId = ToDepartmentId;  
                         return entity;
                     }).ToList();

            // Fetching Current User
            string currentIp = _ipAddressService.GetSystemIPAddress();
            int userId = _ipAddressService.GetUserId();
            string username = _ipAddressService.GetUserName();
            var systemTimeZoneId = _timeZoneService.GetSystemTimeZone();
            var currentTime = _timeZoneService.GetCurrentTime(systemTimeZoneId);
            //Map the Command to Entity
             var assetTransferReceiptHdr = _imapper.Map<FAM.Domain.Entities.AssetMaster.AssetTransferReceiptHdr>(request.AssetTransferReceiptHdrDto);
            // var assetTransferissueHdr = _imapper.Map<FAM.Domain.Entities.AssetMaster.AssetTransferIssueHdr>(request.AssetTransferReceiptHdrDto.AssetTransferIssueHdr);
             
            
             assetTransferReceiptHdr.AuthorizedIP = currentIp;
             assetTransferReceiptHdr.AuthorizedDate = currentTime;
             assetTransferReceiptHdr.AuthorizedBy = userId;
             assetTransferReceiptHdr.AuthorizedByName = username;
             // Fetch existing receipt to check if it's an update or create operation
            var existingReceipt = await _iassettransferreceiptqueryrepository
                .GetByAssetReceiptId(assetTransferReceiptHdr.AssetTransferId);
            var result =  await _iassettransferreceiptcommandrepository.CreateAsync(assetTransferReceiptHdr,assetLocation);
              //Domain Event
                  var domainEvent = new AuditLogsDomainEvent(
                      actionDetail: "Create",
                      actionCode: assetTransferReceiptHdr.Id.ToString(),
                      actionName: "Asset Transfer Receipt",
                      details: $"Asset Transfer Receipt '{assetTransferReceiptHdr.Id}' was created. ",
                      module:"Asset Transfer"
                  );
                  await _imediator.Publish(domainEvent, cancellationToken);
                  if (result > 0)
                  {
                       return  result;
                               
                    }
                    throw new Exception("Asset Transfer Receipt could not be processed");
                 
        }
    }
}