using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDispatchAdvice;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.DispatchAdvice.Dto;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.DispatchAdvice.Queries.GetDispatchAdvicePackRange
{
    public class GetDispatchAdvicePackRangeQueryHandler : IRequestHandler<GetDispatchAdvicePackRangeQuery, List<DispatchAdvicePackRangeDto>>
    {
        private readonly IDispatchAdviceQueryRepository _queryRepository;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetDispatchAdvicePackRangeQueryHandler(
            IDispatchAdviceQueryRepository queryRepository,
            IMiscMasterQueryRepository miscMasterQueryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<List<DispatchAdvicePackRangeDto>> Handle(GetDispatchAdvicePackRangeQuery request, CancellationToken cancellationToken)
        {
            // Always include Packed; user-selected defect MiscMaster Ids are appended.
            var packedStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.StockStatus, MiscEnumEntity.Packed);
            var packedId = packedStatus?.Id ?? 0;

            var statusIds = new List<int> { packedId };
            if (request.DefectStatusIds is { Count: > 0 })
            {
                statusIds.AddRange(request.DefectStatusIds.Where(id => id > 0 && id != packedId));
            }
            statusIds = statusIds.Distinct().ToList();

            var result = await _queryRepository.GetPackRangeAsync(
                request.ItemId, request.LotId, request.PackTypeId, statusIds, request.Range, request.OrderType, request.SourceUnitId);

            // Build a (StatusId → Description) dictionary so each row gets the right StatusName.
            // Packed is known up front; for each unique non-Packed StatusId actually returned,
            // fetch the MiscMaster row by Id.
            var statusNameDict = new Dictionary<int, string?>
            {
                [packedId] = packedStatus?.Description ?? packedStatus?.Code
            };

            var defectIdsInResult = result
                .Select(r => r.StatusId)
                .Where(id => id > 0 && id != packedId)
                .Distinct()
                .ToList();

            foreach (var defectId in defectIdsInResult)
            {
                var defectMisc = await _miscMasterQueryRepository.GetByIdAsync(defectId);
                statusNameDict[defectId] = defectMisc?.Description ?? defectMisc?.Code;
            }

            foreach (var row in result)
            {
                if (statusNameDict.TryGetValue(row.StatusId, out var name))
                    row.StatusName = name;
            }

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetDispatchAdvicePackRangeQuery",
                actionCode: "Get",
                actionName: $"ItemId:{request.ItemId},LotId:{request.LotId},PackTypeId:{request.PackTypeId}",
                details: "Dispatch Advice pack range details were fetched.",
                module: "DispatchAdvice");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
