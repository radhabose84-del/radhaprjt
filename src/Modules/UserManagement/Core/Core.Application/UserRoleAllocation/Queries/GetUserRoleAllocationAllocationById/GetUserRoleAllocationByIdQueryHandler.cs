using Core.Application.UserRoleAllocation.Queries.GetUserRoleAllocation;
using Core.Application.Common.Interfaces;
using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Core.Application.Common.Interfaces.IUserRoleAllocation;


namespace Core.Application.UserRoleAllocation.Queries.GetUserRoleAllocationById
{
    public class GetUserRoleAllocationByIdQueryHandler :IRequestHandler<GetUserRoleAllocationByIdQuery,CreateUserRoleAllocationDto>
    {

        private readonly IUserRoleAllocationQueryRepository _userRoleAllocationRepository;
        private readonly IMapper _mapper;
      
        public GetUserRoleAllocationByIdQueryHandler(IUserRoleAllocationQueryRepository userRoleAllocationRepository, IMapper mapper)
        {
            _userRoleAllocationRepository = userRoleAllocationRepository;
            _mapper = mapper;
        }

        public async Task<CreateUserRoleAllocationDto?> Handle(GetUserRoleAllocationByIdQuery request, CancellationToken cancellationToken)
        {
            if (request.UserId <= 0)
            {
                throw new ArgumentException("Invalid User ID provided.");
            }

            // Fetch roles associated with the user from the repository
            var userRoleAllocations = await _userRoleAllocationRepository.GetByUserIdAsync(request.UserId);
            var roleIds = userRoleAllocations.Select(ura => ura.UserRoleId).ToList();

            // Handle case where no roles are found
            if (roleIds == null || !roleIds.Any())
            {
                return null;
            }

            // Create and return the DTO with the UserId and associated RoleIds
            return new CreateUserRoleAllocationDto
            {
                UserId = request.UserId,
                RoleIds = roleIds.ToList()
            };
          }

        
    }
}