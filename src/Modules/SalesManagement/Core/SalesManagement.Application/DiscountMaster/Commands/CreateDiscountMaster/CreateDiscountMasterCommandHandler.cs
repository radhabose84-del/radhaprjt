using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDiscountMaster;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Events;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.DiscountMaster.Commands.CreateDiscountMaster
{
    public class CreateDiscountMasterCommandHandler : IRequestHandler<CreateDiscountMasterCommand, ApiResponseDTO<int>>
    {
        private readonly IDiscountMasterCommandRepository _commandRepository;
        private readonly IDiscountMasterQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateDiscountMasterCommandHandler(
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

        public async Task<ApiResponseDTO<int>> Handle(CreateDiscountMasterCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.DiscountMaster>(request);

            // Map child collections
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

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "DISCOUNT_MASTER_CREATE",
                actionName: entity.DiscountCode ?? string.Empty,
                details: $"DiscountMaster '{entity.DiscountCode}' created successfully with Id {newId}.",
                module: "DiscountMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "DiscountMaster created successfully.",
                Data = newId
            };
        }
    }
}
