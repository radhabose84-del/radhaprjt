using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IMachineGroupUser;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.MachineGroupUser.Queries.GetMachineGroupUser
{
    public class GetMachineGroupUserQueryHandler : IRequestHandler<GetMachineGroupUserQuery, ApiResponseDTO<List<MachineGroupUserDto>>>
    {
        private readonly IMachineGroupUserQueryRepository _machineGroupQuery;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IDepartmentLookup _departmentLookup;
        private readonly IUserLookup _userLookup;

        public GetMachineGroupUserQueryHandler(
            IMachineGroupUserQueryRepository machineGroupQuery,
            IMapper mapper,
            IMediator mediator,
            IDepartmentLookup departmentLookup,
            IUserLookup userLookup)
        {
            _machineGroupQuery = machineGroupQuery;
            _mapper = mapper;
            _mediator = mediator;
            _departmentLookup = departmentLookup;
            _userLookup = userLookup;
        }
        public async Task<ApiResponseDTO<List<MachineGroupUserDto>>> Handle(GetMachineGroupUserQuery request, CancellationToken cancellationToken)
        {
            var (machineGroupUser, totalCount) = await _machineGroupQuery.GetAllMachineGroupUserAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var machineGroupUserList = _mapper.Map<List<MachineGroupUserDto>>(machineGroupUser);

            var departments = await _departmentLookup.GetAllDepartmentAsync();
            var departmentLookup = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);

            foreach (var data in machineGroupUserList)
            {
                if (departmentLookup.TryGetValue(data.DepartmentId, out var departmentName) && departmentName != null)
                {
                    data.DepartmentName = departmentName;
                }
            }

            var userIds = machineGroupUserList.Select(m => m.UserId).Distinct().ToList();
            if (userIds.Any())
            {
                var userLookups = await _userLookup.GetByIdsAsync(userIds);
                var userLookupDict = userLookups.ToDictionary(u => u.UserId, u => u.UserName);
                foreach (var data in machineGroupUserList)
                {
                    if (userLookupDict.TryGetValue(data.UserId, out var userName))
                        data.UserName = userName;
                }
            }

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetMachineGroupUser",
                    actionCode: "",
                    actionName: "",
                    details: $"MachineGroupUser details was fetched.",
                    module: "MachineGroupUser"
                );
            await _mediator.Publish(domainEvent, cancellationToken);
            return new ApiResponseDTO<List<MachineGroupUserDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = machineGroupUserList,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
