using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDiscountMaster;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Events;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.DiscountMaster.Commands.UpdateDiscountMaster
{
    public class UpdateDiscountMasterCommandHandler : IRequestHandler<UpdateDiscountMasterCommand, ApiResponseDTO<int>>
    {
        private readonly IDiscountMasterCommandRepository _commandRepository;
        private readonly IDiscountMasterQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateDiscountMasterCommandHandler(
            IDiscountMasterCommandRepository commandRepository,
            IDiscountMasterQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateDiscountMasterCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.DiscountMaster>(request);

            // Map child collections (replace strategy — old children removed in repository)
            if (request.Slabs != null && request.Slabs.Count > 0)
            {
                entity.DiscountSlabs = request.Slabs.Select(s => new DiscountSlab
                {
                    SlabOrder = s.SlabOrder,
                    FromValue = s.FromValue,
                    ToValue = s.ToValue,
                    DiscountValue = s.DiscountValue,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                }).ToList();
            }

            if (request.SalesGroupIds != null && request.SalesGroupIds.Count > 0)
            {
                entity.DiscountSalesGroups = request.SalesGroupIds.Select(id => new DiscountSalesGroup
                {
                    SalesGroupId = id,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                }).ToList();
            }

            if (request.PaymentTermIds != null && request.PaymentTermIds.Count > 0)
            {
                entity.DiscountPaymentTerms = request.PaymentTermIds.Select(id => new DiscountPaymentTerm
                {
                    PaymentTermId = id,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                }).ToList();
            }

            var result = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "DISCOUNT_MASTER_UPDATE",
                actionName: request.Id.ToString(),
                details: $"DiscountMaster with Id {request.Id} updated successfully.",
                module: "DiscountMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "DiscountMaster updated successfully.",
                Data = result
            };
        }
    }
}
