using MediatR;
using Core.Application.Common.Interfaces;
using Core.Domain.Entities;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.Interfaces.IUserRole;
using Core.Application.DeleteUserRoleAllocation.Commands.DeleteUserRoleAllocation;
using Core.Application.Common.Interfaces.IUserRoleAllocation;

namespace Core.Application.UserRoleAllocation.Commands.DeleteUserRoleAllocation
{
    public class DeleteRoleAllocationCommandHandler  :IRequestHandler<DeleteRoleAllocationCommand ,int>
    {
    
        private readonly IUserRoleAllocationCommandRepository _IuserroleAllocationRepository;  
        private readonly IUserRoleAllocationQueryRepository _IuserAllocationRoleQueryRepository;
        private readonly IMapper _mapper;
      
      public DeleteRoleAllocationCommandHandler (IUserRoleCommandRepository roleAllocationRepository,IUserRoleAllocationQueryRepository roleAllocationQueryRepository , IMapper mapper)
      {
        _IuserroleAllocationRepository =(IUserRoleAllocationCommandRepository) roleAllocationRepository;
        _IuserAllocationRoleQueryRepository = roleAllocationQueryRepository;
         _mapper = mapper;
      }

       public async Task<int>Handle(DeleteRoleAllocationCommand request, CancellationToken cancellationToken)
      {       
        var allocation = await _IuserAllocationRoleQueryRepository.GetByIdAsync(request.RoleAllocationId);
        if (allocation == null)
        {
            return 0; // Or throw an exception, e.g., NotFoundException
        }

        // Delete the record
        await _IuserroleAllocationRepository.DeleteAsync(request.RoleAllocationId);

        return 1;          
      }


    }
}