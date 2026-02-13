using System;
using System.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Polly;
using Serilog;
using MassTransit;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Common.Interfaces.AuditLog;
using UserManagement.Domain.Common;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Persistence;
using UserManagement.Infrastructure.Repositories;
using UserManagement.Infrastructure.Repositories.AdminSecuritySettings;
using UserManagement.Infrastructure.Repositories.City;
using UserManagement.Infrastructure.Repositories.Companies;
using UserManagement.Infrastructure.Repositories.CompanySettings;
using UserManagement.Infrastructure.Repositories.Country;
using UserManagement.Infrastructure.Repositories.Currency;
using UserManagement.Infrastructure.Repositories.CustomFields;
using UserManagement.Infrastructure.Repositories.DepartmentGroup;
using UserManagement.Infrastructure.Repositories.Departments;
using UserManagement.Infrastructure.Repositories.Divisions;
using UserManagement.Infrastructure.Repositories.Entities;
using UserManagement.Infrastructure.Repositories.FinancialYear;
using UserManagement.Infrastructure.Repositories.Language;
using UserManagement.Infrastructure.Repositories.Menu;
using UserManagement.Infrastructure.Repositories.MiscMaster;
using UserManagement.Infrastructure.Repositories.MiscTypeMaster;
using UserManagement.Infrastructure.Repositories.Module;
using UserManagement.Infrastructure.Repositories.Notifications;
using UserManagement.Infrastructure.Repositories.PasswordComplexityRule;
using UserManagement.Infrastructure.Repositories.Profile;
using UserManagement.Infrastructure.Repositories.RoleEntitlements;
using UserManagement.Infrastructure.Repositories.State;
using UserManagement.Infrastructure.Repositories.TimeZones;
using UserManagement.Infrastructure.Repositories.Units;
using UserManagement.Infrastructure.Repositories.UserGroup;
using UserManagement.Infrastructure.Repositories.UserRoleAllocation.UserRoleAllocationCommandRepository;
using UserManagement.Infrastructure.Repositories.UserRoleAllocation.UserRoleAllocationQueryRepository;
using UserManagement.Infrastructure.Repositories.UserRoles;
using UserManagement.Infrastructure.Repositories.Users;
using UserManagement.Infrastructure.Services;

using UserManagement.Application.Common.Interfaces.IAdminSecuritySettings;
using UserManagement.Application.Common.Interfaces.ICity;
using UserManagement.Application.Common.Interfaces.ICompany;
using UserManagement.Application.Common.Interfaces.ICompanySettings;
using UserManagement.Application.Common.Interfaces.ICountry;
using UserManagement.Application.Common.Interfaces.ICurrency;
using UserManagement.Application.Common.Interfaces.ICustomField;
using UserManagement.Application.Common.Interfaces.IDepartment;
using UserManagement.Application.Common.Interfaces.IDepartmentGroup;
using UserManagement.Application.Common.Interfaces.IDivision;
using UserManagement.Application.Common.Interfaces.IEntity;
using UserManagement.Application.Common.Interfaces.IFinancialYear;
using UserManagement.Application.Common.Interfaces.ILanguage;
using UserManagement.Application.Common.Interfaces.IMenu;
using UserManagement.Application.Common.Interfaces.IMiscMaster;
using UserManagement.Application.Common.Interfaces.IMiscTypeMaster;
using UserManagement.Application.Common.Interfaces.IModule;
using UserManagement.Application.Common.Interfaces.INotifications;
using UserManagement.Application.Common.Interfaces.IPasswordComplexityRule;
using UserManagement.Application.Common.Interfaces.IProfile;
using UserManagement.Application.Common.Interfaces.IState;
using UserManagement.Application.Common.Interfaces.ITimeZones;
using UserManagement.Application.Common.Interfaces.IUnit;
using UserManagement.Application.Common.Interfaces.IUser;
using UserManagement.Application.Common.Interfaces.IUserGroup;
using UserManagement.Application.Common.Interfaces.IUserRole;
using UserManagement.Application.Common.Interfaces.IUserRoleAllocation;
using UserManagement.Application.Common.Interfaces.IUserSession;
using UserManagement.Application.Common.Interfaces.IRoleEntitlement;
using UserManagement.Application.Notification.Queries;

using UserManagement.Domain.Entities;
using MongoDB.Driver.Linq;
using Infrastructure.Data;
using Microsoft.Extensions.Hosting;
using Contracts.Interfaces.Lookups.Users;
using UserManagement.Infrastructure.Repositories.Lookups.Users;
namespace UserManagement.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddUserManagementInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            var isTesting = environment.IsEnvironment("Testing");

            // --------------------------
            // SQL Connection
            // --------------------------
            var raw = configuration.GetConnectionString("DefaultConnection");

            string connectionString;

            if (isTesting)
            {
                // In Testing: avoid env placeholder replacement failures.
                // Keep a valid-looking connection string so IDbConnection can be constructed.
                connectionString = !string.IsNullOrWhiteSpace(raw)
                    ? raw
                    : @"Server=(localdb)\MSSQLLocalDB;Database=UserManagement_TestDb;Trusted_Connection=True;MultipleActiveResultSets=True;";
            }
            else
            {
                if (string.IsNullOrWhiteSpace(raw))
                    throw new InvalidOperationException("Connection string 'DefaultConnection' not found or is empty.");

                connectionString = raw
                    .Replace("{SERVER}", Environment.GetEnvironmentVariable("DATABASE_SERVER") ?? "")
                    .Replace("{USER_ID}", Environment.GetEnvironmentVariable("DATABASE_USERID") ?? "")
                    .Replace("{ENC_PASSWORD}", Environment.GetEnvironmentVariable("DATABASE_PASSWORD") ?? "");

                // Guard: placeholders should not remain
                if (connectionString.Contains("{SERVER}", StringComparison.OrdinalIgnoreCase) ||
                    connectionString.Contains("{USER_ID}", StringComparison.OrdinalIgnoreCase) ||
                    connectionString.Contains("{ENC_PASSWORD}", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        "Database env variables are missing. Check DATABASE_SERVER / DATABASE_USERID / DATABASE_PASSWORD.");
                }
            }

            // Always register DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                if (isTesting)
                {
                    options.UseInMemoryDatabase("UserManagement_TestDb");
                }
                else
                {
                    options.UseSqlServer(connectionString, sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    });
                }
            });

            // Dapper connection (scoped per request)
            // NOTE: In Testing this is only to satisfy DI; avoid calling real DB from unit tests.
            services.AddScoped<IDbConnection>(_ => new SqlConnection(connectionString));

            // DbContext base reference
            services.AddScoped<DbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

            // --------------------------
            // MongoDB (AuditLog + Outbox)
            // --------------------------
            // In Testing: allow missing config; use defaults so DI doesn't fail.
            var mongoConn = configuration.GetConnectionString("MongoDbConnectionString");
            var mongoDbName = configuration["MongoDb:DatabaseName"];

            if (!isTesting)
            {
                if (string.IsNullOrWhiteSpace(mongoConn))
                    throw new InvalidOperationException("MongoDbConnectionString is missing.");

                if (string.IsNullOrWhiteSpace(mongoDbName))
                    throw new InvalidOperationException("MongoDb:DatabaseName is missing.");
            }
            else
            {
                mongoConn = string.IsNullOrWhiteSpace(mongoConn) ? "mongodb://localhost:27017" : mongoConn;
                mongoDbName = string.IsNullOrWhiteSpace(mongoDbName) ? "UserManagement_TestDb" : mongoDbName;
            }

            services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConn));

            services.AddSingleton<IMongoDatabase>(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                return client.GetDatabase(mongoDbName);
            });

            services.AddSingleton<IMongoDbContext>(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                return new MongoDbContext(client, mongoDbName);
            });

            services.AddSingleton<IMongoCollection<OutboxMessage>>(sp =>
            {
                var db = sp.GetRequiredService<IMongoDatabase>();
                return db.GetCollection<OutboxMessage>("OutboxMessages");
            });

            // --------------------------
            // MassTransit
            // --------------------------
            if (isTesting)
            {
                // InMemory bus for tests so IPublishEndpoint resolves
                services.AddMassTransit(x =>
                {
                    x.SetKebabCaseEndpointNameFormatter();
                    x.UsingInMemory((ctx, cfg) =>
                    {
                        cfg.ConfigureEndpoints(ctx);
                    });
                });
            }
            else
            {
                var rabbitHost = configuration["MassTransit:RabbitMq:Host"];
                var rabbitUser = configuration["MassTransit:RabbitMq:Username"];
                var rabbitPass = configuration["MassTransit:RabbitMq:Password"];

                if (string.IsNullOrWhiteSpace(rabbitHost))
                    throw new InvalidOperationException("MassTransit:RabbitMq:Host is missing.");

                rabbitUser = string.IsNullOrWhiteSpace(rabbitUser) ? "guest" : rabbitUser;
                rabbitPass = string.IsNullOrWhiteSpace(rabbitPass) ? "guest" : rabbitPass;

                var rabbitUri = rabbitHost.StartsWith("rabbitmq://", StringComparison.OrdinalIgnoreCase)
                    ? new Uri(rabbitHost)
                    : new Uri($"rabbitmq://{rabbitHost}");

                services.AddMassTransit(x =>
                {
                    x.SetKebabCaseEndpointNameFormatter();
                    x.UsingRabbitMq((ctx, cfg) =>
                    {
                        cfg.Host(rabbitUri, h =>
                        {
                            h.Username(rabbitUser);
                            h.Password(rabbitPass);
                        });

                        cfg.ConfigureEndpoints(ctx);
                    });
                });
            }

            // --------------------------
            // Logging
            // --------------------------
            services.AddLogging(builder => builder.AddSerilog());

            // --------------------------
            // JWT settings
            // --------------------------
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

            // --------------------------
            // HttpClient (BackgroundServiceClient)
            // --------------------------
            // In Testing: allow missing base address.
            services.AddHttpClient("BackgroundServiceClient", client =>
            {
                var baseAddress = configuration["HttpClientSettings:BackgroundService"];

                if (string.IsNullOrWhiteSpace(baseAddress))
                {
                    if (!isTesting)
                        throw new InvalidOperationException("HttpClientSettings:BackgroundService is missing in configuration.");

                    baseAddress = "http://localhost";
                }

                client.BaseAddress = new Uri(baseAddress);
            })
            .AddTransientHttpErrorPolicy(policyBuilder =>
                policyBuilder.CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 3,
                    durationOfBreak: TimeSpan.FromSeconds(30)))
            .AddTransientHttpErrorPolicy(policyBuilder =>
                policyBuilder.WaitAndRetryAsync(3, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));

            services.AddScoped<IBackgroundServiceClient, BackgroundServiceClient>();

            // --------------------------
            // Repositories
            // --------------------------
            services.AddScoped<IUserQueryRepository, UserQueryRepository>();
            services.AddScoped<IUserCommandRepository, UserCommandRepository>();

            services.AddScoped<IUserRoleAllocationQueryRepository, UserRoleAllocationQueryRepository>();
            services.AddScoped<IUserRoleAllocationCommandRepository, UserRoleAllocationCommandRepository>();

            services.AddScoped<IRoleEntitlementCommandRepository, RoleEntitlementCommandRepository>();
            services.AddScoped<IRoleEntitlementQueryRepository, RoleEntitlementQueryRepository>();

            services.AddScoped<IModuleCommandRepository, ModuleCommandRepository>();
            services.AddScoped<IModuleQueryRepository, ModuleQueryRepository>();

            services.AddScoped<IDepartmentCommandRepository, DepartmentCommandRepository>();
            services.AddScoped<IDepartmentQueryRepository, DepartmentQueryRepository>();
             //Lookups
            services.AddScoped<IDepartmentLookup, DepartmentLookupRepository>();
            services.AddScoped<ICompanyLookup, CompanyLookupRepository>();
            services.AddScoped<IUserLookup, UserLookupRepository>();
            services.AddScoped<IDepartmentGroupLookup, DepartmentGroupLookupRepository>();
            services.AddScoped<IFinancialYearLookup, FinancialYearLookupRepository>();
            services.AddScoped<ICurrencyLookup, CurrencyLookupRepository>();

            services.AddScoped<IUserRoleCommandRepository, UserRoleCommandRepository>();
            services.AddScoped<IUserRoleQueryRepository, UserRoleQueryRepository>();

            services.AddScoped<ICompanyCommandRepository, CompanyCommandRepository>();
            services.AddScoped<ICompanyQueryRepository, CompanyQueryRepository>();

            services.AddScoped<IUnitCommandRepository, UnitCommandRepository>();
            services.AddScoped<IUnitQueryRepository, UnitQueryRepository>();
            services.AddScoped<IUnitLookup, UnitLookupRepository>();

            services.AddScoped<IEntityCommandRepository, EntityCommandRepository>();
            services.AddScoped<IEntityQueryRepository, EntityQueryRepository>();

            services.AddScoped<IDivisionCommandRepository, DivisionCommandRepository>();
            services.AddScoped<IDivisionQueryRepository, DivisionQueryRepository>();

            services.AddScoped<ICountryCommandRepository, CountryCommandRepository>();
            services.AddScoped<ICountryQueryRepository, CountryQueryRepository>();
            services.AddScoped<ICountryLookup, CountryLookupRepository>();

            services.AddScoped<IStateCommandRepository, StateCommandRepository>();
            services.AddScoped<IStateQueryRepository, StateQueryRepository>();
            services.AddScoped<IStateLookup, StateLookupRepository>();

            services.AddScoped<ICityCommandRepository, CityCommandRepository>();
            services.AddScoped<ICityQueryRepository, CityQueryRepository>();
            services.AddScoped<ICityLookup, CityLookupRepository>();
            services.AddScoped<ILocationLookup, LocationLookupRepository>();
            services.AddScoped<IDivisionUnitLookup, DivisionUnitLookupRepository>();

            services.AddScoped<IAuditLogRepository, AuditLogRepository>();
            services.AddScoped<IUserSessionRepository, UserSessionRepository>();

            services.AddTransient<NotificationsQueryHandler>();
            services.AddTransient<INotificationsQueryRepository, NotificationsQueryRepository>();

            services.AddScoped<IPasswordComplexityRuleQueryRepository, PasswordComplexityRuleQueryRepository>();
            services.AddScoped<IPasswordComplexityRuleCommandRepository, PasswordComplexityRuleCommandRepository>();

            services.AddScoped<IAdminSecuritySettingsQueryRepository, AdminSecuritySettingsQueryRepository>();
            services.AddScoped<IAdminSecuritySettingsCommandRepository, AdminSecuritySettingsCommandRepository>();

            services.AddScoped<IFinancialYearQueryRepository, FinancialYearQueryRepository>();
            services.AddScoped<IFinancialYearCommandRepository, FinancialYearCommandRepository>();

            services.AddScoped<ICompanyQuerySettings, CompanySettingsQueryRepository>();
            services.AddScoped<ICompanyCommandSettings, CompanySettingsCommandRepository>();

            services.AddScoped<ICurrencyQueryRepository, CurrencyQueryRepository>();
            services.AddScoped<ICurrencyCommandRepository, CurrencyCommandRepository>();

            services.AddScoped<ITimeZonesQueryRepository, TimeZonesQueryRepository>();

            services.AddScoped<ILanguageCommand, LanguageCommandRepository>();
            services.AddScoped<ILanguageQuery, LanguageQueryRepository>();

            services.AddScoped<IMenuQuery, MenuQueryRepository>();
            services.AddScoped<IMenuCommand, MenuCommandRepository>();

            services.AddScoped<IProfileQuery, ProfileQueryRepository>();
            services.AddScoped<IProfileCommand, ProfileCommandRepository>();

            services.AddScoped<IUserGroupQueryRepository, UserGroupQueryRepository>();
            services.AddScoped<IUserGroupCommandRepository, UserGroupCommandRepository>();

            services.AddScoped<ICustomFieldQuery, CustomFieldQuery>();
            services.AddScoped<ICustomFieldCommand, CustomFieldCommand>();

            services.AddScoped<ILoginPolicyFactory, LoginPolicyFactory>();
            services.AddScoped<ILoginPolicy, SuperAdminLoginPolicy>();
            services.AddScoped<ILoginPolicy, UserLoginPolicy>();

            services.AddScoped<IMiscMasterCommandRepository, MiscMasterCommandRepository>();
            services.AddScoped<IMiscMasterQueryRepository, MiscMasterQueryRepository>();
            services.AddScoped<IMiscTypeMasterCommandRepository, MiscTypeMasterCommandRepository>();
            services.AddScoped<IMiscTypeMasterQueryRepository, MiscTypeMasterQueryRepository>();

            services.AddScoped<IDepartmentGroupCommandRepository, DepartmentGroupCommandRepository>();
            services.AddScoped<IDepartmentGroupQueryRepository, DepartmentGroupQueryRepository>();

            // --------------------------
            // Services
            // --------------------------
            services.AddHttpContextAccessor();
            services.AddScoped<IIPAddressService, IPAddressService>();
            services.AddTransient<IFileUploadService, FileUploadRepository>();
            services.AddTransient<IJwtTokenHelper, JwtTokenHelper>();
            services.AddSingleton<ITimeZoneService, TimeZoneService>();
            services.AddScoped<IChangePassword, PasswordChangeRepository>();

            services.AddScoped<IEmailService, EmailSenderService>();
            services.AddScoped<ISmsService, SmsSenderService>();

            // Outbox publisher
            services.AddScoped<IEventPublisher, EventPublisher>();

            // IMPORTANT: keep AutoMapper registration in Application/Module, not Infrastructure.

            return services;
        }
    }
}
