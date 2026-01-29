using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FAM.Application.AssetMaster.AssetTranferIssueApproval.Queries.GetAssetTransferIssueApproval;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.IAssetTransferIssueApproval;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.AssetMaster.AssetTranferIssueApproval.Queries.GetAssetTransferIssueById
{
    public class GetAssetTransferIssueByIdQueryHandler : IRequestHandler<GetAssetTransferIssueByIdQuery, List<AssetTransferIssueByIdDto>>
    {
        private readonly IAssetTransferIssueApprovalQueryRepository _assetTransferIssueQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAssetTransferIssueByIdQueryHandler(IAssetTransferIssueApprovalQueryRepository assetTransferIssueQueryRepository, IMapper mapper, IMediator mediator)
        {
            _assetTransferIssueQueryRepository = assetTransferIssueQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

       public async Task<List<AssetTransferIssueByIdDto>> Handle(GetAssetTransferIssueByIdQuery request, CancellationToken cancellationToken)
    {
        var result = await _assetTransferIssueQueryRepository.GetByAssetTransferIdAsync(request.Id);

        // Check if data exists
        if (result is null || !result.Any())
        {
            throw new ValidationException($"No records found for ID {request.Id}.");
          
        }

        // Map list of results
        var assetTransferIssueList = _mapper.Map<List<AssetTransferIssueByIdDto>>(result);

        // Domain Event Logging
        var domainEvent = new AuditLogsDomainEvent(
            actionDetail: "GetById",
            actionCode: "AssetTransferIssue",
            actionName: request.Id.ToString(),
            details: $"Asset transfer details for ID {request.Id} were fetched.",
            module: "AssetTransferIssue"
        );
        await _mediator.Publish(domainEvent, cancellationToken);

        return  assetTransferIssueList;
    }
    }
}