using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using MediatR;
using UserManagement.Infrastructure;


// Validation infra (still in API for now)
using UserManagement.API.Validation.Common;

// AutoMapper profiles
using Core.Application.Common.Mappings;
using Shared.Validation.Common;

namespace UserManagement.Module;

public static class ModuleExtensions
{
    public static IServiceCollection AddUserManagementModule(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        // ✅ 0) Infrastructure FIRST (DbContext + repos + services)
        services.AddUserManagementInfrastructure(configuration, environment);


        // 1) MediatR (handlers are in Core.Application now)
        // services.AddMediatR(cfg =>
        // {
        //     cfg.RegisterServicesFromAssembly(
        //         typeof(Core.Application.Users.Commands.CreateUser.CreateUserCommand).Assembly);
        // });

        // 2) Validators
        services.AddValidatorsFromAssembly(
            typeof(UserManagement.API.Validation.Companies.CreateCompanyCommandValidator).Assembly);

        // 3) AutoMapper (keep ONE place)
        services.AddAutoMapper(
            typeof(UserProfile),
            typeof(RoleEntitlementMappingProfile),
            typeof(ModuleProfile),
            typeof(ChangePasswordProfile),
            typeof(PasswordComplexityRuleProfile),
            typeof(EntityProfile),
            typeof(AdminSecuritySettingsProfile),
            typeof(DepartmentProfile),
            typeof(FinancialYearProfile),
            typeof(CurrencyProfile),
            typeof(UnitsProfile),
            typeof(CompanySettingsProfile),
            typeof(DepartmentGroupProfile)
        );

        // 4) Validation infrastructure (needed by validators)
        services.AddScoped<MaxLengthProvider>();
        services.AddScoped<IMaxLengthProvider>(sp => sp.GetRequiredService<MaxLengthProvider>());
        // services.AddScoped<IValidationRuleProvider, JsonValidationRuleProvider>();

        return services;
    }
}

// // UserManagement.Module/ModuleExtensions.cs
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Configuration;
// using FluentValidation;

// // Interfaces
// using Core.Application.Common.Interfaces;
// using Core.Application.Common.Interfaces.IUser;
// using Core.Application.Common.Interfaces.IUserRole;
// using Core.Application.Common.Interfaces.ICompany;
// using Core.Application.Common.Interfaces.IDepartment;
// using Core.Application.Common.Interfaces.IMenu;
// using Core.Application.Common.Interfaces.IEntity;
// using Core.Application.Common.Interfaces.IUnit;
// using Core.Application.Common.Interfaces.IDivision;
// using Core.Application.Common.Interfaces.ICountry;
// using Core.Application.Common.Interfaces.IState;
// using Core.Application.Common.Interfaces.ICity;
// using Core.Application.Common.Interfaces.IUserRoleAllocation;
// using Core.Application.Common.Interfaces.IRoleEntitlement;
// using Core.Application.Common.Interfaces.IModule;
// using Core.Application.Common.Interfaces.IPasswordComplexityRule;
// using Core.Application.Common.Interfaces.IAdminSecuritySettings;
// using Core.Application.Common.Interfaces.IFinancialYear;
// using Core.Application.Common.Interfaces.ICompanySettings;
// using Core.Application.Common.Interfaces.ICurrency;
// using Core.Application.Common.Interfaces.ITimeZones;
// using Core.Application.Common.Interfaces.ILanguage;
// using Core.Application.Common.Interfaces.IUserGroup;
// using Core.Application.Common.Interfaces.ICustomField;
// using Core.Application.Common.Interfaces.IMiscMaster;
// using Core.Application.Common.Interfaces.IMiscTypeMaster;
// using Core.Application.Common.Interfaces.IDepartmentGroup;
// using Core.Application.Common.Interfaces.IProfile;
// using Core.Application.Common.Interfaces.AuditLog;
// using Core.Application.Common.Interfaces.INotifications;
// using Core.Application.Common.Interfaces.IUserSession;

// // Repositories
// using UserManagement.Infrastructure.Repositories;
// using UserManagement.Infrastructure.Repositories.Users;
// using UserManagement.Infrastructure.Repositories.Departments;
// using UserManagement.Infrastructure.Repositories.Companies;
// using UserManagement.Infrastructure.Repositories.UserRoles;
// using UserManagement.Infrastructure.Repositories.Menu;
// using UserManagement.Infrastructure.Repositories.Entities;
// using UserManagement.Infrastructure.Repositories.Units;
// using UserManagement.Infrastructure.Repositories.Divisions;
// using UserManagement.Infrastructure.Repositories.Country;
// using UserManagement.Infrastructure.Repositories.State;
// using UserManagement.Infrastructure.Repositories.City;
// using UserManagement.Infrastructure.Repositories.UserRoleAllocation.UserRoleAllocationCommandRepository;
// using UserManagement.Infrastructure.Repositories.UserRoleAllocation.UserRoleAllocationQueryRepository;
// using UserManagement.Infrastructure.Repositories.RoleEntitlements;
// using UserManagement.Infrastructure.Repositories.Module;
// using UserManagement.Infrastructure.Repositories.PasswordComplexityRule;
// using UserManagement.Infrastructure.Repositories.AdminSecuritySettings;
// using UserManagement.Infrastructure.Repositories.FinancialYear;
// using UserManagement.Infrastructure.Repositories.CompanySettings;
// using UserManagement.Infrastructure.Repositories.Currency;
// using UserManagement.Infrastructure.Repositories.TimeZones;
// using UserManagement.Infrastructure.Repositories.Language;
// using UserManagement.Infrastructure.Repositories.UserGroup;
// using UserManagement.Infrastructure.Repositories.CustomFields;
// using UserManagement.Infrastructure.Repositories.MiscMaster;
// using UserManagement.Infrastructure.Repositories.MiscTypeMaster;
// using UserManagement.Infrastructure.Repositories.DepartmentGroup;
// using UserManagement.Infrastructure.Repositories.Profile;
// using UserManagement.Infrastructure.Repositories.Notifications;

// // Services
// using UserManagement.Infrastructure.Services;

// // Validation
// using UserManagement.API.Validation.Common;

// // AutoMapper Profiles
// using Core.Application.Common.Mappings;

// namespace UserManagement.Module;

// /// <summary>
// /// Extension methods for registering UserManagement module services
// /// </summary>
// public static class ModuleExtensions
// {
//     /// <summary>
//     /// Registers all UserManagement module services, repositories, validators, and configurations
//     /// </summary>
//     /// <param name="services">Service collection</param>
//     /// <param name="configuration">Application configuration</param>
//     /// <returns>Service collection for chaining</returns>
//     public static IServiceCollection AddUserManagementModule(
//         this IServiceCollection services,
//         IConfiguration configuration)
//     {
//         // ═══════════════════════════════════════════════════════════
//         // 1. MEDIATR - Command & Query Handlers
//         // ═══════════════════════════════════════════════════════════

//         services.AddMediatR(cfg =>
//         {
//             // Register all MediatR handlers from Core.Application
//             cfg.RegisterServicesFromAssembly(
//                 typeof(Core.Application.Users.Commands.CreateUser.CreateUserCommand).Assembly);
//         });

//         // ═══════════════════════════════════════════════════════════
//         // 2. FLUENTVALIDATION - Validators (Assembly Scanning)
//         // ═══════════════════════════════════════════════════════════

//         // ✅ Automatically register ALL validators from UserManagement.API assembly
//         services.AddValidatorsFromAssembly(
//             typeof(UserManagement.API.Validation.Companies.CreateCompanyCommandValidator).Assembly);


//         // ═══════════════════════════════════════════════════════════
//         // 3. AUTOMAPPER - Mapping Profiles
//         // ═══════════════════════════════════════════════════════════

//         services.AddAutoMapper(
//             typeof(UserProfile),
//             typeof(RoleEntitlementMappingProfile),
//             typeof(ModuleProfile),
//             typeof(ChangePasswordProfile),
//             typeof(PasswordComplexityRuleProfile),
//             typeof(EntityProfile),
//             typeof(AdminSecuritySettingsProfile),
//             typeof(DepartmentProfile),
//             typeof(FinancialYearProfile),
//             typeof(CurrencyProfile),
//             typeof(UnitsProfile),
//             typeof(CompanySettingsProfile),
//             typeof(DepartmentGroupProfile)
//         );

//         // ═══════════════════════════════════════════════════════════
//         // 4. REPOSITORIES - Command/Query Separation (CQRS)
//         // ═══════════════════════════════════════════════════════════

//         // User
//         services.AddScoped<IUserQueryRepository, UserQueryRepository>();
//         services.AddScoped<IUserCommandRepository, UserCommandRepository>();

//         // Department
//         services.AddScoped<IDepartmentQueryRepository, DepartmentQueryRepository>();
//         services.AddScoped<IDepartmentCommandRepository, DepartmentCommandRepository>();

//         // Company
//         services.AddScoped<ICompanyQueryRepository, CompanyQueryRepository>();
//         services.AddScoped<ICompanyCommandRepository, CompanyCommandRepository>();

//         // UserRole
//         services.AddScoped<IUserRoleQueryRepository, UserRoleQueryRepository>();
//         services.AddScoped<IUserRoleCommandRepository, UserRoleCommandRepository>();

//         // Menu
//         services.AddScoped<IMenuQuery, MenuQueryRepository>();
//         services.AddScoped<IMenuCommand, MenuCommandRepository>();

//         // Entity
//         services.AddScoped<IEntityQueryRepository, EntityQueryRepository>();
//         services.AddScoped<IEntityCommandRepository, EntityCommandRepository>();

//         // Unit
//         services.AddScoped<IUnitQueryRepository, UnitQueryRepository>();
//         services.AddScoped<IUnitCommandRepository, UnitCommandRepository>();

//         // Division
//         services.AddScoped<IDivisionQueryRepository, DivisionQueryRepository>();
//         services.AddScoped<IDivisionCommandRepository, DivisionCommandRepository>();

//         // Country
//         services.AddScoped<ICountryQueryRepository, CountryQueryRepository>();
//         services.AddScoped<ICountryCommandRepository, CountryCommandRepository>();

//         // State
//         services.AddScoped<IStateQueryRepository, StateQueryRepository>();
//         services.AddScoped<IStateCommandRepository, StateCommandRepository>();

//         // City
//         services.AddScoped<ICityQueryRepository, CityQueryRepository>();
//         services.AddScoped<ICityCommandRepository, CityCommandRepository>();

//         // UserRoleAllocation
//         services.AddScoped<IUserRoleAllocationQueryRepository, UserRoleAllocationQueryRepository>();
//         services.AddScoped<IUserRoleAllocationCommandRepository, UserRoleAllocationCommandRepository>();

//         // RoleEntitlement
//         services.AddScoped<IRoleEntitlementQueryRepository, RoleEntitlementQueryRepository>();
//         services.AddScoped<IRoleEntitlementCommandRepository, RoleEntitlementCommandRepository>();

//         // Module
//         services.AddScoped<IModuleQueryRepository, ModuleQueryRepository>();
//         services.AddScoped<IModuleCommandRepository, ModuleCommandRepository>();

//         // Password & Security
//         services.AddScoped<IPasswordComplexityRuleQueryRepository, PasswordComplexityRuleQueryRepository>();
//         services.AddScoped<IPasswordComplexityRuleCommandRepository, PasswordComplexityRuleCommandRepository>();

//         services.AddScoped<IAdminSecuritySettingsQueryRepository, AdminSecuritySettingsQueryRepository>();
//         services.AddScoped<IAdminSecuritySettingsCommandRepository, AdminSecuritySettingsCommandRepository>();

//         // Financial Year
//         services.AddScoped<IFinancialYearQueryRepository, FinancialYearQueryRepository>();
//         services.AddScoped<IFinancialYearCommandRepository, FinancialYearCommandRepository>();

//         // Company Settings
//         services.AddScoped<ICompanyQuerySettings, CompanySettingsQueryRepository>();
//         services.AddScoped<ICompanyCommandSettings, CompanySettingsCommandRepository>();

//         // Currency
//         services.AddScoped<ICurrencyQueryRepository, CurrencyQueryRepository>();
//         services.AddScoped<ICurrencyCommandRepository, CurrencyCommandRepository>();

//         // TimeZones
//         services.AddScoped<ITimeZonesQueryRepository, TimeZonesQueryRepository>();

//         // Language
//         services.AddScoped<ILanguageCommand, LanguageCommandRepository>();
//         services.AddScoped<ILanguageQuery, LanguageQueryRepository>();

//         // Profile
//         services.AddScoped<IProfileQuery, ProfileQueryRepository>();
//         services.AddScoped<IProfileCommand, ProfileCommandRepository>();

//         // UserGroup
//         services.AddScoped<IUserGroupQueryRepository, UserGroupQueryRepository>();
//         services.AddScoped<IUserGroupCommandRepository, UserGroupCommandRepository>();

//         // CustomField
//         services.AddScoped<ICustomFieldQuery, CustomFieldQuery>();
//         services.AddScoped<ICustomFieldCommand, CustomFieldCommand>();

//         // MiscMaster & MiscTypeMaster
//         services.AddScoped<IMiscMasterCommandRepository, MiscMasterCommandRepository>();
//         services.AddScoped<IMiscMasterQueryRepository, MiscMasterQueryRepository>();
//         services.AddScoped<IMiscTypeMasterCommandRepository, MiscTypeMasterCommandRepository>();
//         services.AddScoped<IMiscTypeMasterQueryRepository, MiscTypeMasterQueryRepository>();

//         // DepartmentGroup
//         services.AddScoped<IDepartmentGroupCommandRepository, DepartmentGroupCommandRepository>();
//         services.AddScoped<IDepartmentGroupQueryRepository, DepartmentGroupQueryRepository>();

//         // MongoDB Repositories
//         services.AddScoped<IAuditLogRepository, AuditLogRepository>();
//         services.AddScoped<IUserSessionRepository, UserSessionRepository>();
//         services.AddScoped<INotificationsQueryRepository, NotificationsQueryRepository>();

//         // ═══════════════════════════════════════════════════════════
//         // 5. SERVICES - Domain Services & Infrastructure Services
//         // ═══════════════════════════════════════════════════════════

//         services.AddScoped<IChangePassword, PasswordChangeRepository>();
//         services.AddScoped<IJwtTokenHelper, JwtTokenHelper>();
//         services.AddScoped<IEmailService, EmailSenderService>();
//         services.AddScoped<ISmsService, SmsSenderService>();
//         services.AddScoped<IFileUploadService, FileUploadRepository>();

//         // Login Policies
//         services.AddScoped<ILoginPolicyFactory, LoginPolicyFactory>();
//         services.AddScoped<ILoginPolicy, SuperAdminLoginPolicy>();
//         services.AddScoped<ILoginPolicy, UserLoginPolicy>();

//         // ═══════════════════════════════════════════════════════════
//         // 6. VALIDATION INFRASTRUCTURE
//         // ═══════════════════════════════════════════════════════════

//         services.AddScoped<MaxLengthProvider>();
//         services.AddScoped<IMaxLengthProvider>(sp => 
//             sp.GetRequiredService<MaxLengthProvider>());
//         services.AddScoped<IValidationRuleProvider, JsonValidationRuleProvider>();

//         // ❌ REMOVED: ValidationService registration
//         // No longer needed - using assembly scanning instead

//         return services;
//     }
// }
