// using Contracts.Dtos.Users;
// using Contracts.Dtos.Maintenance;
// using Contracts.Common;
// using Core.Application.Users.Commands.CreateUser;
// using Core.Application.Users.Commands.UpdateUser;
// using Core.Application.State.Queries.GetStates;

// namespace Core.Application.Common.Interfaces;

// /// <summary>
// /// Interface for cross-module communication with UserManagement module
// /// Other modules can depend on this interface to interact with UserManagement
// /// without creating a circular dependency
// /// </summary>
// public interface IUserManagementModule
// {
//     // ============================================
//     // USER OPERATIONS
//     // ============================================
    
//     /// <summary>
//     /// Get user by ID
//     /// </summary>
//     Task<Result<UserDto>> GetUserByIdAsync(int userId, CancellationToken ct);
    
//     /// <summary>
//     /// Get users by department
//     /// </summary>
//     Task<Result<List<UserDto>>> GetUsersByDepartmentAsync(int departmentId, CancellationToken ct);
    
//     /// <summary>
//     /// Create a new user
//     /// </summary>
//     Task<Result<int>> CreateUserAsync(CreateUserCommand command, CancellationToken ct);
    
//     /// <summary>
//     /// Update an existing user
//     /// </summary>
//     Task<Result<bool>> UpdateUserAsync(UpdateUserCommand command, CancellationToken ct);

//     // ============================================
//     // DEPARTMENT OPERATIONS
//     // ============================================
    
//     /// <summary>
//     /// Get department by ID
//     /// </summary>
//     Task<Result<DepartmentDto>> GetDepartmentByIdAsync(int departmentId, CancellationToken ct);
    
//     /// <summary>
//     /// Get all departments
//     /// </summary>
//     Task<Result<List<DepartmentDto>>> GetDepartmentsAsync(CancellationToken ct);

//     // ============================================
//     // COMPANY OPERATIONS
//     // ============================================
    
//     /// <summary>
//     /// Get company by ID
//     /// </summary>
//     Task<Result<CompanyDto>> GetCompanyByIdAsync(int companyId, CancellationToken ct);

//     // ============================================
//     // VALIDATION (Called by other modules)
//     // ============================================
    
//     /// <summary>
//     /// Validate if a department exists
//     /// Used by other modules to check department validity
//     /// </summary>
//     Task<Result<bool>> ValidateDepartmentExistsAsync(int departmentId, CancellationToken ct);
    
//     /// <summary>
//     /// Validate if a user exists
//     /// Used by other modules to check user validity
//     /// </summary>
//     Task<Result<bool>> ValidateUserExistsAsync(int userId, CancellationToken ct);
    
//     /// <summary>
//     /// Validate if a company exists
//     /// Used by other modules to check company validity
//     /// </summary>
//     Task<Result<bool>> ValidateCompanyExistsAsync(int companyId, CancellationToken ct);

//     // ============================================
//     // LOCATION OPERATIONS
//     // ============================================
    
//     /// <summary>
//     /// Get city by ID
//     /// </summary>
//     Task<Result<CityDto>> GetCityByIdAsync(int cityId, CancellationToken ct);
    
//     /// <summary>
//     /// Get state by ID
//     /// </summary>
//     Task<Result<StateDto>> GetStateByIdAsync(int stateId, CancellationToken ct);
    
//     /// <summary>
//     /// Get country by ID
//     /// </summary>
//     Task<Result<CountryDto>> GetCountryByIdAsync(int countryId, CancellationToken ct);
// }
