using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using FinanceManagement.Application.CoaChangeRequest.Dto;
using FinanceManagement.Application.Common.Interfaces.ICoaChangeRequest;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.CoaChangeRequest.Queries.GetPostFreezeChangeLog
{
    public class GetPostFreezeChangeLogQueryHandler
        : IRequestHandler<GetPostFreezeChangeLogQuery, List<PostFreezeChangeLogDto>>
    {
        private readonly ICoaChangeRequestQueryRepository _queryRepository;
        private readonly IUserLookup _userLookup;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;

        public GetPostFreezeChangeLogQueryHandler(
            ICoaChangeRequestQueryRepository queryRepository,
            IUserLookup userLookup,
            IIPAddressService ipAddressService,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _userLookup = userLookup;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
        }

        public async Task<List<PostFreezeChangeLogDto>> Handle(
            GetPostFreezeChangeLogQuery request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId() ?? 0;
            var rows = await _queryRepository.GetPostFreezeChangeLogAsync(companyId, cancellationToken);

            // Enrich both approver names via the cross-module user lookup (no cross-module JOIN — Rule #3).
            var userIds = rows.SelectMany(r => new[] { r.CfoApproverUserId, r.SysAdminApproverUserId })
                              .Where(id => id.HasValue)
                              .Select(id => id!.Value)
                              .Distinct()
                              .ToList();

            if (userIds.Count > 0)
            {
                var users = await _userLookup.GetByIdsAsync(userIds, cancellationToken);
                var byId = users.ToDictionary(u => u.UserId, u => $"{u.FirstName} {u.LastName}".Trim());

                foreach (var r in rows)
                {
                    if (r.CfoApproverUserId.HasValue && byId.TryGetValue(r.CfoApproverUserId.Value, out var cfoName))
                        r.CfoApproverName = cfoName;
                    if (r.SysAdminApproverUserId.HasValue && byId.TryGetValue(r.SysAdminApproverUserId.Value, out var saName))
                        r.SysAdminApproverName = saName;
                }
            }

            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetPostFreezeChangeLogQuery",
                actionName: rows.Count.ToString(),
                details: "Post-freeze change log was fetched.",
                module: "CoaChangeRequest"), cancellationToken);

            return rows;
        }
    }
}
