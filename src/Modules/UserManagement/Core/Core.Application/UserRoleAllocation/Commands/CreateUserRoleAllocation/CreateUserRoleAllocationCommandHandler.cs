using Core.Application.UserRoleAllocation.Queries.GetUserRoleAllocation;
using Core.Domain.Entities;
using Core.Application.Common.Interfaces;
using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.Interfaces.IUserRoleAllocation;

namespace Core.Application.UserRoleAllocation.Commands.CreateUserRoleAllocation
{
    public class CreateUserRoleAllocationCommandHandler :IRequestHandler<CreateUserRoleAllocationCommand, List<UserRoleAllocationResponseDto>>
    {
        
        private readonly IUserRoleAllocationCommandRepository _userRoleAllocationRepository;
        private readonly IMapper _mapper;

        public CreateUserRoleAllocationCommandHandler(IUserRoleAllocationCommandRepository roleAllocationRepository,IMapper mapper)
        {
             _userRoleAllocationRepository = roleAllocationRepository;
            _mapper = mapper;
        }

        public async Task<List<UserRoleAllocationResponseDto>>Handle(CreateUserRoleAllocationCommand request,CancellationToken cancellationToken)
        {          
            var allocations = request.UserRoleAllocationDto.RoleIds
            .Select(roleId => new Core.Domain.Entities.UserRoleAllocation
            {
                UserId = request.UserRoleAllocationDto.UserId,
                UserRoleId = roleId
            })
            .ToList();

        await _userRoleAllocationRepository.AddRangeAsync(allocations);

        var response = _mapper.Map<List<UserRoleAllocationResponseDto>>(allocations);
        return response;

         }



    }
}