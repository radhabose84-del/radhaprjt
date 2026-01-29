using AutoMapper;
using Core.Application.Common.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Domain.Entities;
using System.Data;
using Core.Application.Common.Interfaces.IUserRoleAllocation;


namespace Core.Application.UserRoleAllocation.Queries.GetUserRoleAllocation
{
    public class GetUserRoleAllocationQueryHandler :IRequestHandler<GetUserRoleAllocationQuery,List<CreateUserRoleAllocationDto>>
    {
        private readonly IUserRoleAllocationQueryRepository _userRoleAllocationRepository;
        private readonly IMapper _mapper;

       public GetUserRoleAllocationQueryHandler(IUserRoleAllocationQueryRepository userRoleAllocationRepository, IMapper mapper)
        {
            _userRoleAllocationRepository = userRoleAllocationRepository;
            _mapper = mapper;
        }

        public async Task<List<CreateUserRoleAllocationDto>> Handle(GetUserRoleAllocationQuery request ,CancellationToken cancellationToken )
        {
            var userRoleAllocations = await _userRoleAllocationRepository.GetAllAsync();

            // Map data to the DTO
            var allocationDtos = _mapper.Map<List<CreateUserRoleAllocationDto>>(userRoleAllocations);

            return allocationDtos;
        }



  }
}