using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAssetTransfered;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetTransferIssue;
using FAM.Domain.Entities.AssetMaster;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.AssetMaster.AssetTransferIssue.Command.CreateAssetTransferIssue
{
    public class CreateAssetTransferIssueCommandHandler : IRequestHandler<CreateAssetTransferIssueCommand,int>
    {
       private readonly  IAssetTransferCommandRepository _assetTransferCommandRepository;
        private readonly IMapper _mapper;  
        private readonly IMediator _Imediator;
         private readonly IIPAddressService _ipAddressService;
        private readonly ITimeZoneService _timeZoneService; 
        private readonly IValidator<CreateAssetTransferIssueCommand> _validator; 
      
    
        public CreateAssetTransferIssueCommandHandler(IAssetTransferCommandRepository assetTransferCommandRepository , IMapper mapper, IMediator Imediator, IIPAddressService ipAddressService, ITimeZoneService timeZoneService, IValidator<CreateAssetTransferIssueCommand> validator)
        {
            _assetTransferCommandRepository = assetTransferCommandRepository;
            _mapper = mapper;      
            _Imediator = Imediator;      
            _ipAddressService = ipAddressService;
            _timeZoneService = timeZoneService;
            _validator = validator;       

        }
     public async Task<int> Handle(CreateAssetTransferIssueCommand request, CancellationToken cancellationToken)
        {
                
            string currentIp = _ipAddressService.GetSystemIPAddress();
            int userId = _ipAddressService.GetUserId(); 
            string username = _ipAddressService.GetUserName();
            var systemTimeZoneId = _timeZoneService.GetSystemTimeZone();
            var currentTime = _timeZoneService.GetCurrentTime(systemTimeZoneId); 
            // 🔹 Map Command to Entity
             var assetTransferIssueHdr = _mapper.Map<FAM.Domain.Entities.AssetMaster.AssetTransferIssueHdr>(request.AssetTransferIssueHdrDto); 
             assetTransferIssueHdr.CreatedIP = currentIp;
             assetTransferIssueHdr.CreatedDate = currentTime;
             assetTransferIssueHdr.CreatedBy = userId;
             assetTransferIssueHdr.CreatedByName = username;
              var result =  await _assetTransferCommandRepository.CreateAssetTransferAsync(assetTransferIssueHdr);

            

              //Domain Event
                  var domainEvent = new AuditLogsDomainEvent(
                      actionDetail: "Create",
                      actionCode: assetTransferIssueHdr.Id.ToString(),
                      actionName: "Asset Transfer",
                      details: $"Asset Transfer '{assetTransferIssueHdr.Id}' was created. ",
                      module:"Asset Transfer"
                  );     

                  await _Imediator.Publish(domainEvent, cancellationToken);
                  if (result > 0)
                  {
                     
                        return result;
                 }
                 throw new Exception("Asset Transfer not created");
        }
        
    }
}