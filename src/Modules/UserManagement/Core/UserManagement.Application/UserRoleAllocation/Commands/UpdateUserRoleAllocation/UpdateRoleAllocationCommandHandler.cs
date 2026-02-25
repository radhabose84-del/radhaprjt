using MediatR;
using AutoMapper;
using UserManagement.Application.Common.Interfaces.IUserRoleAllocation;
using UserManagement.Application.UserRoleAllocation.Queries.GetUserRoleAllocation;

namespace UserManagement.Application.UserRoleAllocation.Commands.UpdateUserRoleAllocation
{
    public class UpdateRoleAllocationCommandHandler  : IRequestHandler<UpdateRoleAllocationCommand ,List<UserRoleAllocationResponseDto>>  
    {


        public readonly IUserRoleAllocationCommandRepository _IUserRoleAllocationRepository;
        private readonly IUserRoleAllocationQueryRepository _IuserAllocationRoleQueryRepository;
        private readonly IMapper _Imapper;
        public UpdateRoleAllocationCommandHandler(IUserRoleAllocationCommandRepository roleAllocationRepository,IUserRoleAllocationQueryRepository userAllocationRoleQueryRepository ,IMapper mapper)
        {
            _IUserRoleAllocationRepository = roleAllocationRepository;
            _IuserAllocationRoleQueryRepository = userAllocationRoleQueryRepository;
            _Imapper = mapper;
        }

        public async Task<List<UserRoleAllocationResponseDto>> Handle(UpdateRoleAllocationCommand request, CancellationToken cancellationToken)
        {
       // Fetch existing allocations for the user
        var existingAllocations = await _IuserAllocationRoleQueryRepository.GetByUserIdAsync(request.UserId);

        // Identify roles to delete
        var rolesToDelete = existingAllocations
            .Where(a => !request.NewRoleIds.Contains(a.UserRoleId))
            .ToList();

        // Identify roles to add
        var existingRoleIds = existingAllocations.Select(a => a.UserRoleId).ToList();
        var rolesToAdd = request.NewRoleIds
            .Where(newRoleId => !existingRoleIds.Contains(newRoleId))
            .Select(newRoleId => new UserManagement.Domain.Entities.UserRoleAllocation
            {
                UserId = request.UserId,
                UserRoleId = newRoleId
            })
            .ToList();

        // Delete roles
        foreach (var role in rolesToDelete)
        {
            await _IUserRoleAllocationRepository.DeleteAsync(role.Id);
        }

        // Add new roles
        await _IUserRoleAllocationRepository.AddRangeAsync(rolesToAdd);

        // Fetch updated allocations for response
        var updatedAllocations = await _IuserAllocationRoleQueryRepository.GetByUserIdAsync(request.UserId);

        // Map to response DTO
        var response = _Imapper.Map<List<UserRoleAllocationResponseDto>>(updatedAllocations);

        return response;

       }
        


    }
}