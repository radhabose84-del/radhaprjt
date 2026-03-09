using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDispatchAdvice;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.DispatchAdvice.Dto;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.DispatchAdvice.Queries.GetDispatchAdvicePackNoValidation
{
    public class GetDispatchAdvicePackNoValidationQueryHandler : IRequestHandler<GetDispatchAdvicePackNoValidationQuery, PackNoValidationDto>
    {
        private readonly IDispatchAdviceQueryRepository _queryRepository;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetDispatchAdvicePackNoValidationQueryHandler(
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

        public async Task<PackNoValidationDto> Handle(GetDispatchAdvicePackNoValidationQuery request, CancellationToken cancellationToken)
        {
            var packedStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.StockStatus, MiscEnumEntity.Packed);
            var statusId = packedStatus?.Id ?? 0;

            var availablePackNos = await _queryRepository.GetAvailablePackNosAsync(
                request.ItemId, request.LotId, statusId, request.StartPackNo, request.EndPackNo, request.PackTypeId);

            var requestedPackNos = Enumerable.Range(request.StartPackNo, request.EndPackNo - request.StartPackNo + 1).ToList();
            var missingPackNos = requestedPackNos.Except(availablePackNos).ToList();

            var result = new PackNoValidationDto();

            if (missingPackNos.Count > 0)
            {
                result.IsValid = false;
                result.MissingPackNos = missingPackNos;
                result.Message = $"Stock not found for Pack No(s): {string.Join(", ", missingPackNos)}";
            }
            else
            {
                result.IsValid = true;
                result.Message = "All pack numbers are available in stock.";
            }

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetDispatchAdvicePackNoValidationQuery",
                actionCode: "Get",
                actionName: $"ItemId:{request.ItemId},LotId:{request.LotId},PackNo:{request.StartPackNo}-{request.EndPackNo}",
                details: $"Pack No validation: {result.Message}",
                module: "DispatchAdvice");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
