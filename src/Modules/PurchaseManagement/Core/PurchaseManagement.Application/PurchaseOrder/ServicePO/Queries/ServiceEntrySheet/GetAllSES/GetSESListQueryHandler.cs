using AutoMapper;
using Contracts.Common;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ServicePO;
using PurchaseManagement.Domain.Common;
using MediatR;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;

namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetAllSES
{
    public class GetSESListQueryHandler : IRequestHandler<GetSESListQuery, ApiResponseDTO<List<GetServiceEntrySheetListDto>>>
    {

        private readonly IServicePurchaseOrderQueryRepository _servicePurchaseOrderQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IUnitLookup _unitLookup;
        private readonly IPartyLookup _partyLookup;
         private readonly IWorkflowLookup _workflowLookup;
         private readonly IUserLookup _usersAllLookup;
        private readonly IIPAddressService _ipAddressService;



        public GetSESListQueryHandler(IServicePurchaseOrderQueryRepository servicePurchaseOrderQueryRepository, IMapper mapper, IMediator mediator, IUnitLookup unitLookup, IPartyLookup partyLookup 
            , IWorkflowLookup workflowLookup, IUserLookup usersAllLookup, IIPAddressService ipAddressService)
        {
            _servicePurchaseOrderQueryRepository = servicePurchaseOrderQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            _unitLookup = unitLookup;
            _partyLookup = partyLookup;
            _workflowLookup = workflowLookup;
            _usersAllLookup = usersAllLookup;
            _ipAddressService = ipAddressService;

        }
        
            public async Task<ApiResponseDTO<List<GetServiceEntrySheetListDto>>> Handle(
            GetSESListQuery request,
            CancellationToken cancellationToken)
        {
            // 1️⃣ Get paged SES rows from repo
            var (rows, total) = await _servicePurchaseOrderQueryRepository
                .GetAllServiceEntrySheetsAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            // if nothing, return early
            if (rows == null || !rows.Any())
            {
                return new ApiResponseDTO<List<GetServiceEntrySheetListDto>>
                {
                    IsSuccess  = false,
                    Message    = "No Service Entry Sheets found.",
                    Data       = new List<GetServiceEntrySheetListDto>(),
                    TotalCount = 0,
                    PageNumber = request.PageNumber,
                    PageSize   = request.PageSize,
                    StatusCode = 200
                };
            }

            // 2️⃣ Collect distinct VendorIds from the page
            var vendorIds = rows
                .Where(r => r.VendorId > 0)
                .Select(r => r.VendorId)
                .Distinct()
                .ToList();

            // 3️⃣ Build a lookup: VendorId -> (Name, Code)
            var vendorLookup = new Dictionary<int, (string Name, string Code)>();
            if (vendorIds.Any())
            {
                var parties = await _partyLookup.GetByIdsAsync(vendorIds, cancellationToken);
                vendorLookup = parties.ToDictionary(
                    p => p.Id,
                    p => (p.PartyName, p.PartyCode));
            }

            // 4️⃣ Enrich each SES row with VendorName / VendorCode
            foreach (var ses in rows)
            {
                if (ses.VendorId > 0 && vendorLookup.TryGetValue(ses.VendorId, out var v))
                {
                    ses.VendorName = v.Name;
                    ses.VendorCode = v.Code;
                }
            }
            


             // 3️⃣ Workflow enrichment: filter by current user + fill Approver / ApprovalRequestHeaderId
            var sesIds = rows
                .Select(r => r.Id)
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            if (sesIds.Count > 0)
            {
                var currentUserId = _ipAddressService.GetUserId();

                // 3.1) Get workflow approvers for ServiceEntrySheet module
                var wfApprovers = await _workflowLookup
                    .GetApproverListAsync(MiscEnumEntity.ServiceEntrySheet.ToString(), sesIds);

                if (wfApprovers != null && wfApprovers.Any())
                {
                    // 🔥 Filter: only SES where current user is the approver
                    var allowedSesIds = wfApprovers
                        .Where(a =>
                            !string.IsNullOrWhiteSpace(a.ApproverValue) &&
                            int.TryParse(a.ApproverValue, out var parsed) &&
                            parsed == currentUserId)
                        .Select(a => a.ModuleTransactionId)
                        .ToHashSet();

                    // Keep only rows for current approver
                    rows = rows.Where(r => allowedSesIds.Contains(r.Id)).ToList();

                    if (!rows.Any())
                    {
                        return new ApiResponseDTO<List<GetServiceEntrySheetListDto>>
                        {
                            IsSuccess  = false,
                            Message    = "No Service Entry Sheets found for current approver.",
                            Data       = new List<GetServiceEntrySheetListDto>(),
                            TotalCount = 0,
                            PageNumber = request.PageNumber,
                            PageSize   = request.PageSize,
                            StatusCode = 200
                        };
                    }

                    // Rebuild SES Ids for remaining rows
                    sesIds = rows.Select(r => r.Id).Distinct().ToList();

                    // 3.2) Build map: SES Id -> { ApproverId, ApprovalRequestId }
                    var wfByModuleId = wfApprovers
                        .Where(a => allowedSesIds.Contains(a.ModuleTransactionId))
                        .GroupBy(a => a.ModuleTransactionId)
                        .ToDictionary(
                            g => g.Key,
                            g =>
                            {
                                var first = g.First();
                                int? approverId = null;
                                if (!string.IsNullOrWhiteSpace(first.ApproverValue) &&
                                    int.TryParse(first.ApproverValue, out var parsed))
                                {
                                    approverId = parsed;
                                }

                                return new
                                {
                                    ApproverId = approverId,
                                    ApprovalRequestId = first.ApprovalRequestId,
                                    IsEdit = first.IsEdit
                                };
                            });

                    // 3.3) Get all users to resolve ApproverName
                    var users = await _usersAllLookup.GetAllUserAsync();
                    var userLookup = users.ToDictionary(u => u.UserId, u => u.UserName);

                    // 3.4) Attach to each SES row
                    foreach (var ses in rows)
                    {
                        if (wfByModuleId.TryGetValue(ses.Id, out var wf))
                        {
                            if (wf.ApproverId.HasValue)
                            {
                                ses.ApproverId = wf.ApproverId.Value;
                                if (userLookup.TryGetValue(ses.ApproverId.Value, out var approverName))
                                {
                                    ses.ApproverName = approverName;
                                }
                            }

                            ses.ApprovalRequestHeaderId = wf.ApprovalRequestId;
                            ses.IsEdit = wf.IsEdit;
                        }
                    }
                }
            }

            // 5️⃣ Wrap in ApiResponseDTO
            return new ApiResponseDTO<List<GetServiceEntrySheetListDto>>
            {
                IsSuccess = true,
                Message = "Service Entry Sheets retrieved successfully.",
                Data = rows,
                TotalCount = total,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                StatusCode = 200
            };
        }
        //  public async Task<ApiResponseDTO<List<GetServiceEntrySheetListDto>>> Handle(
        //     GetSESListQuery request,
        //     CancellationToken cancellationToken)
        // {
        //     var (rows, total) = await _servicePurchaseOrderQueryRepository
        //         .GetAllServiceEntrySheetsAsync(request.PageNumber, request.PageSize, request.SearchTerm);

        //         var response = new ApiResponseDTO<List<GetServiceEntrySheetListDto>>();



        //     return new ApiResponseDTO<List<GetServiceEntrySheetListDto>>
        //     {
        //         IsSuccess = true,
        //         Message = "Service Entry Sheets retrieved successfully.",
        //         Data = rows,
        //         TotalCount = total,
        //         PageNumber = request.PageNumber,
        //         PageSize = request.PageSize,
        //         StatusCode = 200
        //     };
        // }


    }
}
