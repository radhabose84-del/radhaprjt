using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Interfaces.Lookups.Users;
using BudgetManagement.Application.Common.Interfaces.IBudgetAllocation;
using BudgetManagement.Domain.Events;
using MediatR;

namespace BudgetManagement.Application.BudgetAllocation.Queries.GetBudgetBalanceReport
{
    public class GetBudgetBalanceReportQueryHandler : IRequestHandler<GetBudgetBalanceReportQuery, List<BudgetBalanceReportDto>>
    {
        private readonly IBudgetAllocationQueryRepository _repository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IFinancialYearLookup _financialYearLookup;

        public GetBudgetBalanceReportQueryHandler(IBudgetAllocationQueryRepository repository, IMapper mapper, IMediator mediator, IFinancialYearLookup financialYearLookup)
        {
            _repository = repository;
            _mapper = mapper;
            _mediator = mediator;
            _financialYearLookup = financialYearLookup;
        }
        public async Task<List<BudgetBalanceReportDto>> Handle(
            GetBudgetBalanceReportQuery request,
            CancellationToken cancellationToken)
            {
                // 1️⃣ Fetch data
                var result = await _repository.GetBudgetAllocationsAsync(
                    request.FinancialYearId
                );

                // 2️⃣ Publish Domain Event (Audit Log)
                await _mediator.Publish(
                    new AuditLogsDomainEvent(
                        actionDetail: "GetAll",
                        actionCode: nameof(GetBudgetBalanceReportQuery),
                        actionName: request.FinancialYearId.ToString(),
                        details: $"Budget Balance Report fetched for FinancialYearId: {request.FinancialYearId}",
                        module: "Budget"
                    ),
                    cancellationToken
                );

                var financialYearIds = result
                    .Where(r => r.FinancialYearId > 0)
                    .Select(r => r.FinancialYearId)
                    .Distinct()
                    .ToArray();

                if (financialYearIds.Length > 0)
                {
                    var financialYears = await _financialYearLookup.GetByIdsAsync(financialYearIds, cancellationToken);
                    var financialYearLookup = financialYears
                        .Where(fy => fy.FinancialYearName is not null)
                        .ToDictionary(fy => fy.FinancialYearId, fy => fy.FinancialYearName!);

                    foreach (var dto in result)
                    {
                        if (financialYearLookup.TryGetValue(dto.FinancialYearId, out var financialYearName))
                        {
                            dto.FinancialYearName = financialYearName;
                        }
                    }
                }

                return result;
            }
    }
}
