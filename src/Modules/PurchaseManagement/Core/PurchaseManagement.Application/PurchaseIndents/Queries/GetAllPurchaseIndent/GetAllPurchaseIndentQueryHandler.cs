using System.Text.Json;
using AutoMapper;
using Contracts.Common;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseIndent;
using PurchaseManagement.Domain.Events;
using MediatR;
using Contracts.Interfaces.Lookups.Users;

namespace PurchaseManagement.Application.PurchaseIndents.Queries.GetAllPurchaseIndent
{
    public class GetAllPurchaseIndentQueryHandler : IRequestHandler<GetAllPurchaseIndentQuery, ApiResponseDTO<List<PurchaseManagement.Application.PurchaseIndents.Queries.GetAllPurchaseIndent.IndentDto>>>
    {
        private readonly IPurchaseIndentQuery _purchaseIndentQuery;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IDepartmentLookup _departmentLookup;
        private readonly IUnitLookup _unitLookup;
        public GetAllPurchaseIndentQueryHandler(IPurchaseIndentQuery purchaseIndentQuery, IMediator mediator, IMapper mapper,             IDepartmentLookup departmentLookup,
            IUnitLookup unitLookup)
        {
            _purchaseIndentQuery = purchaseIndentQuery;
            _mediator = mediator;
            _mapper = mapper;
            _departmentLookup = departmentLookup;
            _unitLookup = unitLookup;
        }
        public async Task<ApiResponseDTO<List<PurchaseManagement.Application.PurchaseIndents.Queries.GetAllPurchaseIndent.IndentDto>>> Handle(GetAllPurchaseIndentQuery request, CancellationToken cancellationToken)
        {
            var (indents, totalCount) =
            await _purchaseIndentQuery.GetAllPurchaseIndentAsync(
                request.PageNumber,
                request.PageSize,
                request.SearchTerm,
                request.StatusId
            );

        var units = await _unitLookup.GetAllUnitAsync();
        var unitLookup = units.ToDictionary(x => x.UnitId, x => x.UnitName);

        var departments = await _departmentLookup.GetAllDepartmentAsync();
        var deptLookup = departments.ToDictionary(x => x.DepartmentId, x => x.DepartmentName);

        foreach (var indent in indents)
        {
            if (unitLookup.TryGetValue(indent.UnitId, out var unitName))
                indent.UnitName = unitName;

            if (deptLookup.TryGetValue(indent.DepartmentId, out var deptName))
                indent.DepartmentName = deptName;
        }

        await _mediator.Publish(
            new AuditLogsDomainEvent(
                "GetAll",
                "GetAll",
                "GetAll",
                JsonSerializer.Serialize(request),
                "PurchaseIndent"
            ),
            cancellationToken
        );

        return new ApiResponseDTO<List<IndentDto>>
        {
            IsSuccess = true,
            Message = "Success",
            Data = indents,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
           
        }
    }
}