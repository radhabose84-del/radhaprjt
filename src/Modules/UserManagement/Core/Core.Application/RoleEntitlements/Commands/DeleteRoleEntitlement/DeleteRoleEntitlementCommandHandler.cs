using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Core.Application.Common.HttpResponse;
using Core.Application.Common.Interfaces.IRoleEntitlement;
using Core.Application.RoleEntitlements.Queries.GetRoleEntitlements;
using Core.Domain.Entities;
using Core.Domain.Enums.Common;
using Core.Domain.Events;
using MediatR;

namespace Core.Application.RoleEntitlements.Commands.DeleteRoleEntitlement
{
    public class DeleteRoleEntitlementCommandHandler : IRequestHandler<DeleteRoleEntitlementCommand, ApiResponseDTO<RoleEntitlementDto>>
    {
        private readonly IRoleEntitlementCommandRepository _roleEntitlementCommandRepository;
        private readonly IRoleEntitlementQueryRepository _roleEntitlementQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public DeleteRoleEntitlementCommandHandler(
            IRoleEntitlementCommandRepository roleEntitlementCommandRepository, 
            IRoleEntitlementQueryRepository roleEntitlementQueryRepository, 
            IMapper mapper, 
            IMediator mediator)
        {
            _roleEntitlementCommandRepository = roleEntitlementCommandRepository ?? throw new ArgumentNullException(nameof(roleEntitlementCommandRepository));
            _roleEntitlementQueryRepository = roleEntitlementQueryRepository ?? throw new ArgumentNullException(nameof(roleEntitlementQueryRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<ApiResponseDTO<RoleEntitlementDto>> Handle(DeleteRoleEntitlementCommand request, CancellationToken cancellationToken)
        {
            // if (request == null)
            // {
            //     throw new ArgumentNullException(nameof(request), "Request cannot be null.");
            // }

            // // Fetch role entitlement by ID
            // var roleEntitlement = await _roleEntitlementQueryRepository.GetByIdAsync(request.Id);
            // if (roleEntitlement == null || roleEntitlement.IsDeleted == Enums.IsDelete.Deleted)
            // {
            //     return new ApiResponseDTO<RoleEntitlementDto>
            //     {
            //         IsSuccess = false,
            //         Message = "Invalid RoleName. The specified RoleName does not exist."
            //     };
            // }

            // // Soft Delete by setting IsActive = 0
            // // roleEntitlement.IsActive = Enums.Status.Inactive;
            // var roleEntitlementdata = _mapper.Map<RoleEntitlement>(request);
            // // Update the entity in the database
            // var updateResult = await _roleEntitlementCommandRepository.DeleteAsync(roleEntitlement.Id, roleEntitlementdata);
            // if (updateResult > 0)
            // {
            //     var roleEntitlementDto = _mapper.Map<RoleEntitlementDto>(roleEntitlement);

            //     // Domain Event  
            //     var domainEvent = new AuditLogsDomainEvent(
            //         actionDetail: "Delete",
            //         actionCode: roleEntitlementDto.RoleName,
            //         actionName: roleEntitlementDto.ModuleName,
            //         details: $"RoleEntitlement '{roleEntitlementDto.ModuleName}' was deleted.",
            //         module: "RoleEntitlement"
            //     );

            //     await _mediator.Publish(domainEvent, cancellationToken);

            //     return new ApiResponseDTO<RoleEntitlementDto>
            //     {
            //         IsSuccess = true,
            //         Message = "RoleEntitlement deleted successfully.",
            //         Data = roleEntitlementDto
            //     };
            // }

            return new ApiResponseDTO<RoleEntitlementDto>
            {
                IsSuccess = false,
                Message = "RoleEntitlement deletion failed."
            };
        }
    }
}
