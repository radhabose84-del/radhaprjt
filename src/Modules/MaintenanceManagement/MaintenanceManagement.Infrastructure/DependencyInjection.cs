using System.Data;
using MaintenanceManagement.Application.Common.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Serilog;
using MaintenanceManagement.Infrastructure.Data;
using MaintenanceManagement.Infrastructure.Services;
using MaintenanceManagement.Application.Common.Interfaces.IMachineGroup;
using MaintenanceManagement.Infrastructure.Repositories.MachineGroup;
using MaintenanceManagement.Application.Common.Interfaces.IMiscTypeMaster;
using MaintenanceManagement.Infrastructure.Repositories.MiscTypeMaster;
using MaintenanceManagement.Application.Common.Interfaces.IMiscMaster;
using MaintenanceManagement.Infrastructure.Repositories.MiscMaster;
using MaintenanceManagement.Application.Common.Interfaces.IShiftMaster;
using MaintenanceManagement.Infrastructure.Repositories.ShiftMaster;
using MaintenanceManagement.Application.Common.Interfaces.IShiftMasterDetail;
using MaintenanceManagement.Infrastructure.Repositories.ShiftMasterDetailRepo;
using MaintenanceManagement.Application.Common.Interfaces.ICostCenter;
using MaintenanceManagement.Infrastructure.Repositories.CostCenter;
using MaintenanceManagement.Application.Common.Interfaces.IWorkCenter;
using MaintenanceManagement.Infrastructure.Repositories.WorkCenter;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceType;
using MaintenanceManagement.Infrastructure.Repositories.MaintenanceType;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceCategory;
using MaintenanceManagement.Infrastructure.Repositories.MaintenanceCategory;
using MaintenanceManagement.Application.Common.Interfaces.IActivityMaster;
using MaintenanceManagement.Infrastructure.Repositories.ActivityMaster;
using MaintenanceManagement.Application.Common.Interfaces.IMachineMaster;
using MaintenanceManagement.Application.Common.Interfaces.IMachineGroupUser;
using MaintenanceManagement.Infrastructure.Repositories.MachineMaster;
using MaintenanceManagement.Infrastructure.Repositories.MachineGroupUser;
using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MaintenanceManagement.Application.Common.Interfaces.IActivityCheckListMaster;
using MaintenanceManagement.Infrastructure.Repositories.ActivityCheckListMaster;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceRequest;
using MaintenanceManagement.Infrastructure.Repositories.MaintenanceRequest;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
using MaintenanceManagement.Infrastructure.Repositories.PreventiveSchedulers;
using MaintenanceManagement.Infrastructure.Repositories.WorkOrder;
using MaintenanceManagement.Application.Common.Interfaces.IItem;
using MaintenanceManagement.Infrastructure.Repositories.Item;
using MaintenanceManagement.Application.Common.Interfaces.IStcokLedger;
using MaintenanceManagement.Infrastructure.Repositories.StockLedger;
using MaintenanceManagement.Application.Common.Interfaces.IMainStoreStock;
using MaintenanceManagement.Infrastructure.Repositories.MainStoreStock;
using MaintenanceManagement.Application.Common.Interfaces.IMRS;
using MaintenanceManagement.Infrastructure.Repositories.MRS;
using MaintenanceManagement.Infrastructure.Repositories.Reports;
using MaintenanceManagement.Application.Common.Interfaces.IReports;
using MaintenanceManagement.Application.Common.Interfaces.Power.IFeederGroup;
using MaintenanceManagement.Infrastructure.Repositories.Power.FeederGroup;
using MaintenanceManagement.Application.Common.Interfaces.Power.IPowerConsumption;
using MaintenanceManagement.Infrastructure.Repositories.Power.PowerConsumption;
using MaintenanceManagement.Application.Common.Interfaces.Power.IFeeder;
using MaintenanceManagement.Infrastructure.Repositories.Power.Feeder;
using MaintenanceManagement.Application.Common.Interfaces.IDashboard;
using MaintenanceManagement.Infrastructure.Repositories.Dashboard;
using MaintenanceManagement.Application.Common.Interfaces.IMachineSpecification;
using MaintenanceManagement.Infrastructure.Repositories.MachineSpecification;
using MaintenanceManagement.Application.Common.Interfaces.Power.IGeneratorConsumption;
using MaintenanceManagement.Infrastructure.Repositories.Power.GeneratorConsumption;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveSchedulerLog;
using MaintenanceManagement.Infrastructure.Repositories.PreventiveSchedulesLogs;
using MaintenanceManagement.Application.Common.IMachineSpecification;
using Contracts.Interfaces.Lookups.Maintenance;
using MaintenanceManagement.Infrastructure.Repositories.Lookups.Maintenance;
using MaintenanceManagement.Application.Common.Interfaces.IOutbox;
using MaintenanceManagement.Infrastructure.Repositories.Outbox;
using MaintenanceManagement.Infrastructure.Services.Outbox;

using MassTransit;

namespace MaintenanceManagement.Infrastructure
{
    public static class DependencyInjection 
    {
        public static IServiceCollection AddMaintenanceInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = (configuration.GetConnectionString("DefaultConnection") ?? string.Empty)
                                                .Replace("{SERVER}", Environment.GetEnvironmentVariable("DATABASE_SERVER") ?? "")
                                                .Replace("{USER_ID}", Environment.GetEnvironmentVariable("DATABASE_USERID") ?? "")
                                                .Replace("{ENC_PASSWORD}", Environment.GetEnvironmentVariable("DATABASE_PASSWORD") ?? "");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found or is empty.");
            }

            // Register ApplicationDbContext with SQL Server
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                }));

            // Register IDbConnection for Dapper
            services.AddTransient<IDbConnection>(sp => new SqlConnection(connectionString));

            // MongoDB Context
            services.AddSingleton<IMongoClient>(sp =>
            {
                var mongoConnectionString = configuration.GetConnectionString("MongoDbConnectionString");
                if (string.IsNullOrWhiteSpace(mongoConnectionString))
                {
                    throw new InvalidOperationException("MongoDB connection string is missing or empty.");
                }
                return new MongoClient(mongoConnectionString);
            });

            services.AddSingleton<IMongoDbContext>(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                var databaseName = configuration["MongoDb:DatabaseName"];
                if (string.IsNullOrWhiteSpace(databaseName))
                {
                    throw new InvalidOperationException("MongoDB database name is missing or empty.");
                }
                return new MongoDbContext(client, databaseName);
            });

            services.AddSingleton(sp =>
            {
                var mongoDbContext = (MongoDbContext)sp.GetRequiredService<IMongoDbContext>();
                return mongoDbContext.GetDatabase();
            });
            
             services.AddLogging(builder =>
            {
                builder.AddSerilog();
            });

            services.AddHttpContextAccessor();
            services.AddTransient<AuthTokenHandler>();

            // ═══════════════════════════════════════════════════════════════
            // REPOSITORY REGISTRATIONS - ALL REQUIRED REPOSITORIES
            // ═══════════════════════════════════════════════════════════════

            services.AddScoped<ICostCenterQueryRepository, CostCenterQueryRepository>();
            services.AddScoped<ICostCenterCommandRepository, CostCenterCommandRepository>();
            services.AddScoped<ICostCenterLookup, CostCenterLookupRepository>();
            services.AddScoped<IDepartmentValidationLookup, DepartmentValidationLookupRepository>();
            services.AddScoped<IWorkCenterQueryRepository, WorkCenterQueryRepository>();
            services.AddScoped<IWorkCenterCommandRepository, WorkCenterCommandRepository>();
            services.AddScoped<IMachineGroupCommandRepository, MachineGroupCommandRepository>();
            services.AddScoped<IMachineGroupQueryRepository, MachineGroupQueryRepository>();
            services.AddScoped<IMiscTypeMasterCommandRepository, MiscTypeMasterCommandRepository>();
            services.AddScoped<IMiscTypeMasterQueryRepository, MiscTypeMasterQueryRepository>();
            services.AddScoped<IMiscMasterCommandRepository, MiscMasterCommandRepository>();
            services.AddScoped<IMiscMasterQueryRepository, MiscMasterQueryRepository>();
            services.AddScoped<IShiftMasterQuery, ShiftMasterQueryRepository>();
            services.AddScoped<IShiftMasterCommand, ShiftMasterCommandRepository>();
            services.AddScoped<IShiftMasterDetailQuery, ShiftMasterDetailQueryRepository>();
            services.AddScoped<IShiftMasterDetailCommand, ShiftMasterDetailCommandRepository>();
            services.AddScoped<IMaintenanceTypeCommandRepository, MaintenanceTypeCommandRepository>();
            services.AddScoped<IMaintenanceTypeQueryRepository, MaintenanceTypeQueryRepository>();
            services.AddScoped<IMaintenanceCategoryCommandRepository, MaintenanceCategoryCommandRepository>();
            services.AddScoped<IMaintenanceCategoryQueryRepository, MaintenanceCategoryQueryRepository>();
            services.AddScoped<IActivityMasterQueryRepository, ActivityMasterQueryRepository>();
            services.AddScoped<IActivityMasterCommandRepository, ActivityMasterCommandRepository>();
            services.AddScoped<IMachineGroupUserQueryRepository, MachineGroupUserQueryRepository>();
            services.AddScoped<IMachineGroupUserCommandRepository, MachineGroupUserCommandRepository>();
            services.AddScoped<IMachineMasterCommandRepository, MachineMasterCommandRepository>();
            services.AddScoped<IMachineMasterQueryRepository, MachineMasterQueryRepository>();

            // ✅ CRITICAL: WorkOrder repositories
            services.AddScoped<IWorkOrderCommandRepository, WorkOrderCommandRepository>();
            services.AddScoped<IWorkOrderQueryRepository, WorkOrderQueryRepository>();

            services.AddScoped<IActivityCheckListMasterQueryRepository, ActivityCheckListMasterQueryRepository>();
            services.AddScoped<IActivityCheckListMasterCommandRepository, ActivityCheckListMasterCommandRepository>();
            services.AddScoped<IMaintenanceRequestQueryRepository, MaintenanceRequestQueryRepository>();
            services.AddScoped<IMaintenanceRequestCommandRepository, MaintenanceRequestCommandRepository>();

            // ✅ CRITICAL: PreventiveScheduler repositories
            services.AddScoped<IPreventiveSchedulerCommand, PreventiveSchedulerCommandRepository>();
            services.AddScoped<IPreventiveSchedulerQuery, PreventiveSchedulerQueryRepository>();

            services.AddScoped<IItemQueryRepository, ItemQueryRepository>();
            services.AddScoped<IStockLedgerQueryRepository, StockLedgerQueryRepository>();
            services.AddScoped<IMainStoreStockQueryRepository, MainStoreStockQueryRepository>();
            services.AddScoped<IMRSQueryRepository, MRSQueryRepository>();
            services.AddScoped<IMRSCommandRepository, MRSCommandRepository>();
            services.AddScoped<IFeederGroupQueryRepository, FeederGroupQueryRepository>();
            services.AddScoped<IFeederGroupCommandRepository, FeederGroupCommandRepository>();
            services.AddScoped<IPowerConsumptionQueryRepository, PowerConsumptionQueryRepository>();
            services.AddScoped<IPowerConsumptionCommandRepository, PowerConsumptionCommandRepository>();
            services.AddScoped<IFeederQueryRepository, FeederQueryRepository>();
            services.AddScoped<IFeederCommandRepository, FeederCommandRepository>();
            services.AddScoped<IReportRepository, ReportsRepository>();

            // ✅ CRITICAL: Dashboard repository
            services.AddScoped<IDashboardQueryRepository, DashboardQueryRepository>();

            services.AddScoped<IMachineSpecificationCommandRepository, MachineSpecificationCommandRepository>();
            services.AddScoped<IMachineSpecificationQueryRepository, MachineSpecificationQueryRepository>();
            services.AddScoped<IGeneratorConsumptionQueryRepository, GeneratorConsumptionQueryRepository>();
            services.AddScoped<IGeneratorConsumptionCommandRepository, GeneratorConsumptionCommandRepository>();
            services.AddScoped<IPreventiveScheduleLogService, PreventiveScheduleLogsService>();

            // Miscellaneous services
            services.AddTransient<IFileUploadService, FileUploadRepository>();
            services.AddSingleton<ITimeZoneService, TimeZoneService>();
            services.AddTransient<IJwtTokenHelper, JwtTokenHelper>();
            services.AddScoped<ILogQueryService, LogQueryService>();

            var rabbitHost = configuration["MassTransit:RabbitMq:Host"];
            if (string.IsNullOrWhiteSpace(rabbitHost))
            {
                throw new InvalidOperationException("MassTransit:RabbitMq:Host is missing.");
            }

            var rabbitUser = configuration["MassTransit:RabbitMq:Username"];
            var rabbitPass = configuration["MassTransit:RabbitMq:Password"];

            var rabbitUri = rabbitHost.StartsWith("rabbitmq://", StringComparison.OrdinalIgnoreCase)
                ? new Uri(rabbitHost)
                : new Uri($"rabbitmq://{rabbitHost}");

            if (!services.Any(sd => sd.ServiceType == typeof(IBusControl)))
            {
                try
                {
                    services.AddMassTransit(x =>
                    {
                        x.SetKebabCaseEndpointNameFormatter();
                        x.UsingRabbitMq((context, cfg) =>
                        {
                            cfg.Host(rabbitUri, h =>
                            {
                                h.Username(string.IsNullOrWhiteSpace(rabbitUser) ? "guest" : rabbitUser);
                                h.Password(string.IsNullOrWhiteSpace(rabbitPass) ? "guest" : rabbitPass);
                            });

                            cfg.ConfigureEndpoints(context);
                        });
                    });
                }
                catch (MassTransit.ConfigurationException ex)
                {
                    if (ex.Message.Contains("AddMassTransit() was already called", StringComparison.OrdinalIgnoreCase))
                    {
                        // MassTransit already configured by another module; skip additional registration.
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // ═══════════════════════════════════════════════════════════════
            // OUTBOX PATTERN SERVICES
            // ═══════════════════════════════════════════════════════════════

            // Outbox repository (SQL-based for transaction atomicity)
            services.AddScoped<IOutboxRepository, OutboxRepository>();

            // Outbox event publisher (saves events to outbox table)
            services.AddScoped<IOutboxEventPublisher, OutboxEventPublisher>();

            // Unit of work — wraps EF Core transaction for atomic domain writes + outbox insert
            services.AddScoped<IMaintenanceUnitOfWork, MaintenanceUnitOfWork>();

            return services;
        }
    }
}
