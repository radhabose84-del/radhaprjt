#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using FAM.Application.Common.Interfaces;
using FAM.Application.Common.Interfaces.IAssetTransferIssueApproval;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.AssetMaster.AssetTranferIssueApproval.Commands.UpdateAssetTranferIssueApproval
{
    public class UpdateAssetTranferIssueApprovalCommandHandler : IRequestHandler<UpdateAssetTranferIssueApprovalCommand, int>
    {
       private readonly IAssetTransferIssueApprovalCommandRepository _assetTransferIssueApprovalCommandRepository;
        private readonly IMediator _imediator;
        private readonly IMapper _imapper;
        private readonly IIPAddressService _ipAddressService;
        private readonly ITimeZoneService _timeZoneService;
        public UpdateAssetTranferIssueApprovalCommandHandler(IAssetTransferIssueApprovalCommandRepository assetTransferIssueApprovalCommandRepository, IMediator imediator, IMapper imapper, IIPAddressService ipAddressService, ITimeZoneService timeZoneService)
        {
            _assetTransferIssueApprovalCommandRepository=assetTransferIssueApprovalCommandRepository;
            _imediator=imediator;
            _imapper=imapper;
            _ipAddressService=ipAddressService;
            _timeZoneService=timeZoneService;

        }

    public async Task<int> Handle(UpdateAssetTranferIssueApprovalCommand request, CancellationToken cancellationToken)
        {
       

            var transfers = await _assetTransferIssueApprovalCommandRepository.GetByIdsAsync(request.Id);

            if (!transfers.Any())
            {
                throw new ValidationException("Asset transfer records not found.");
            }

            string currentIp = _ipAddressService.GetSystemIPAddress();
            int userId = _ipAddressService.GetUserId();
            string username = _ipAddressService.GetUserName();
            var currentTime = _timeZoneService.GetCurrentTime(_timeZoneService.GetSystemTimeZone());

            // 🔹 Bulk Update in Single Query
            var result = await _assetTransferIssueApprovalCommandRepository.ExecuteBulkUpdateAsync(request.Id, request.Status, userId, currentTime, username, currentIp);

            if (result <= 0)
            {
                throw new Exception("Failed to update asset transfer records.");
            }

            // 🔹 Publish Audit Log
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: string.Join(",", request.Id),
                actionName: request.Status,
                details: $"Asset transfer status updated to {request.Status} for Transfer IDs: {string.Join(",", request.Id)}",
                module: "AssetTransferIssueApproval"
            );
            await _imediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}