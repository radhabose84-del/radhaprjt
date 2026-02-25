using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using UserManagement.Infrastructure;


// Validation infra (still in API for now)
using UserManagement.Presentation.Validation.Common;

// AutoMapper profiles

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


        // 1) MediatR (handlers are in UserManagement.Application now)
        // Use an assembly marker type from UserManagement.Application
        var applicationAssembly = typeof(UserProfile).Assembly; // UserManagement.Application
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));

        // services.AddMediatR(cfg =>
        // {
        //     cfg.RegisterServicesFromAssembly(
        //         typeof(UserManagement.Application.Users.Commands.CreateUser.CreateUserCommand).Assembly);
        // });

        // ✅ 2) Validators (scan API validators assembly)
        var validatorsAssembly = typeof(UserManagement.Presentation.Validation.Companies.CreateCompanyCommandValidator).Assembly;
        services.AddValidatorsFromAssembly(validatorsAssembly);

        // // 2) Validators
        // services.AddValidatorsFromAssembly(
        //     typeof(UserManagement.Presentation.Validation.Companies.CreateCompanyCommandValidator).Assembly);

        // ✅ 3) AutoMapper (scan mapping profiles from Application assembly)
        services.AddAutoMapper(applicationAssembly);

        // // 3) AutoMapper (keep ONE place)
        // services.AddAutoMapper(
        //     typeof(UserProfile),
        //     typeof(RoleEntitlementMappingProfile),
        //     typeof(ModuleProfile),
        //     typeof(ChangePasswordProfile),
        //     typeof(PasswordComplexityRuleProfile),
        //     typeof(EntityProfile),
        //     typeof(AdminSecuritySettingsProfile),
        //     typeof(DepartmentProfile),
        //     typeof(FinancialYearProfile),
        //     typeof(CurrencyProfile),
        //     typeof(UnitsProfile),
        //     typeof(CompanySettingsProfile),
        //     typeof(DepartmentGroupProfile)
        // );
        

        // 4) Validation infrastructure (needed by validators)
        services.AddScoped<MaxLengthProvider>();
        services.AddScoped<IMaxLengthProvider>(sp => sp.GetRequiredService<MaxLengthProvider>());
        // services.AddScoped<IValidationRuleProvider, JsonValidationRuleProvider>();

        return services;
    }
}
