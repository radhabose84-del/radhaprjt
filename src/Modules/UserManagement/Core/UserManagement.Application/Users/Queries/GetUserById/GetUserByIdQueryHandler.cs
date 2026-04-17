#nullable disable
using UserManagement.Application.Users.Queries.GetUsers;
using AutoMapper;
using MediatR;
using UserManagement.Application.Common.Interfaces.IUser;
using UserManagement.Domain.Events;
using Microsoft.Extensions.Logging;

namespace UserManagement.Application.Users.Queries.GetUserById
{
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery,UserByIdDTO>
    {
        private readonly IUserQueryRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ILogger<GetUserByIdQueryHandler> _logger;


        public GetUserByIdQueryHandler(IUserQueryRepository userRepository, IMapper mapper, IMediator mediator,ILogger<GetUserByIdQueryHandler> logger)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _mediator = mediator;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }
        public async Task<UserByIdDTO> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching user details for UserId: {UserId}", request.UserId);
            
            // Fetch the user by ID
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
            {
                _logger.LogWarning("User with UserId: {UserId} not found.", request.UserId);
                return null; // Or throw an exception if preferred
            }
            _logger.LogInformation("User details for UserId: {UserId} successfully fetched.", request.UserId);

           // Publish a domain event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetById",
                    actionCode: user.UserName,        
                    actionName: user.FirstName + " " + user.LastName,                
                    details: $"Fetched details for User '{user.UserName}'. Full Name: {user.FirstName} {user.LastName}",
                    module:"User"
                );

                await _mediator.Publish(domainEvent, cancellationToken);
                var dto = _mapper.Map<UserByIdDTO>(user);

                // Collect all IDs for batch lookup
                var deptIds = new List<int>();
                var companyIds = new List<int>();
                var unitIds = new List<int>();
                var divisionIds = new List<int>();
                var roleIds = new List<int>();

                if (user.DepartmentId > 0) deptIds.Add(user.DepartmentId);
                if (dto.userDepartments != null) deptIds.AddRange(dto.userDepartments.Select(d => d.DepartmentId));
                if (dto.UserCompanies != null) companyIds.AddRange(dto.UserCompanies.Select(c => c.CompanyId));
                if (dto.UserUnits != null) unitIds.AddRange(dto.UserUnits.Select(u => u.UnitId));
                if (dto.userDivisions != null) divisionIds.AddRange(dto.userDivisions.Select(d => d.DivisionId));
                if (dto.userRoleAllocations != null) roleIds.AddRange(dto.userRoleAllocations.Select(r => r.UserRoleId));

                // Batch fetch all names
                var deptNames = await _userRepository.GetDepartmentNamesByIdsAsync(deptIds);
                var ugNames = user.UserGroupId.HasValue && user.UserGroupId.Value > 0
                    ? await _userRepository.GetUserGroupNamesByIdsAsync(new[] { user.UserGroupId.Value })
                    : new Dictionary<int, string>();
                var companyNames = await _userRepository.GetCompanyNamesByIdsAsync(companyIds);
                var unitNames = await _userRepository.GetUnitNamesByIdsAsync(unitIds);
                var divisionNames = await _userRepository.GetDivisionNamesByIdsAsync(divisionIds);
                var roleNames = await _userRepository.GetUserRoleNamesByIdsAsync(roleIds);
                var entityNames = user.EntityId.HasValue && user.EntityId.Value > 0
                    ? await _userRepository.GetEntityNamesByIdsAsync(new[] { user.EntityId.Value })
                    : new Dictionary<int, string>();
                var empNames = user.EmpId.HasValue && user.EmpId.Value > 0
                    ? await _userRepository.GetMarketingOfficerNamesByIdsAsync(new[] { user.EmpId.Value })
                    : new Dictionary<int, string>();

                // Enrich top-level fields
                if (user.DepartmentId > 0 && deptNames.TryGetValue(user.DepartmentId, out var ownerDeptName))
                    dto.DepartmentName = ownerDeptName;
                if (user.UserGroupId.HasValue && ugNames.TryGetValue(user.UserGroupId.Value, out var ugName))
                    dto.UserGroupName = ugName;
                if (user.EntityId.HasValue && entityNames.TryGetValue(user.EntityId.Value, out var entName))
                    dto.EntityName = entName;
                if (user.EmpId.HasValue && empNames.TryGetValue(user.EmpId.Value, out var empName))
                    dto.EmpName = empName;

                // Enrich child collections
                if (dto.userDepartments != null)
                    foreach (var d in dto.userDepartments)
                        if (deptNames.TryGetValue(d.DepartmentId, out var dn)) d.DepartmentName = dn;
                if (dto.UserCompanies != null)
                    foreach (var c in dto.UserCompanies)
                        if (companyNames.TryGetValue(c.CompanyId, out var cn)) c.CompanyName = cn;
                if (dto.UserUnits != null)
                    foreach (var u in dto.UserUnits)
                        if (unitNames.TryGetValue(u.UnitId, out var un)) u.UnitName = un;
                if (dto.userDivisions != null)
                    foreach (var d in dto.userDivisions)
                        if (divisionNames.TryGetValue(d.DivisionId, out var dn)) d.DivisionName = dn;
                if (dto.userRoleAllocations != null)
                    foreach (var r in dto.userRoleAllocations)
                        if (roleNames.TryGetValue(r.UserRoleId, out var rn)) r.RoleName = rn;

                return dto;

        }
    }
}