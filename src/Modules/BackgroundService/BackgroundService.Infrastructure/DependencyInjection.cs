using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BackgroundService.Infrastructure.Configurations;
using BackgroundService.Infrastructure.Services;
using BackgroundService.Application.Interfaces;
using Hangfire;
using Hangfire.SqlServer;
using BackgroundService.Infrastructure.Jobs;
using Polly;
using System.Data;
using BackgroundService.Infrastructure.Repositories.HangFire;
using BackgroundService.Application.Common.Notification.Interfaces;
using BackgroundService.Infrastructure.Repositories.Notification;
using BackgroundService.Infrastructure.Data.Notification;
using Microsoft.EntityFrameworkCore;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationConfig;
using BackgroundService.Infrastructure.Repositories.Common;
using BackgroundService.Infrastructure.Repositories.Notification.NotificationConfig;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationGroup;
using BackgroundService.Infrastructure.Repositories.Notification.NotificationGroup;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationWhatsAppGroup;
using BackgroundService.Infrastructure.Repositories.Notification.NotificationWhatsAppGroup;
using BackgroundService.Application.Notification.Common.Interfaces;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationLevelHierarchy;
using BackgroundService.Infrastructure.Repositories.Notification.NotificationLevelHierarchy;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationTemplate;
using BackgroundService.Infrastructure.Repositories.Notification.NotificationTemplate;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationGroupMembers;
using BackgroundService.Infrastructure.Repositories.Notification.NotificationGroupMember;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationEventRule;

using BackgroundService.Application.Notification.Common.Mappings;
using MassTransit;
using BackgroundService.Application.Consumers;
using BackgroundService.Application.Interfaces.Notification;
using BackgroundService.Infrastructure.Services.Notification;
using BackgroundService.Application.Notification;

using BackgroundService.Application.Workflow.Common.Interfaces.IWorkflowType;
using BackgroundService.Infrastructure.Repositories.Workflow.WorkflowTypes;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalStepDetail;
using BackgroundService.Infrastructure.Repositories.Workflow.ApprovalStepDetails;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalRule;
using BackgroundService.Infrastructure.Repositories.Workflow.ApprovalRules;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationDetail;
using BackgroundService.Infrastructure.Repositories.Notification.NotificationDetail;
using BackgroundService.Application.Consumer.Workflow;
using PurchaseManagement.Application.Consumers;
using BudgetManagement.Application.Consumers;
using InventoryManagement.Application.Consumers;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalRequest;
using BackgroundService.Infrastructure.Repositories.Workflow.ApprovalRequests;
using MongoDB.Driver;
using BackgroundService.Infrastructure.Persistence;
using BackgroundService.Infrastructure.Data;
using BackgroundService.Application.Workflow.Common.Interfaces;
using BackgroundService.Application.Interfaces.IMiscMaster;
using BackgroundService.Infrastructure.Repositories.MiscMaster;
using BackgroundService.Application.Common.Interfaces.IMiscTypeMaster;
using BackgroundService.Infrastructure.Repositories.MiscTypeMaster;
using BackgroundService.Application.Interfaces.IHangfire;
using BackgroundService.Application.Consumer.PreventiveSchedule;
using BackgroundService.Application.Interfaces.Files;
using BackgroundService.Infrastructure.Files;
using BackgroundService.Application.Interfaces.IInbox;
using BackgroundService.Infrastructure.Repositories.Inbox;
using Contracts.Interfaces.Lookups.Workflow;
using BackgroundService.Infrastructure.Repositories.Lookups.Workflow;



namespace BackgroundService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration, IServiceCollection builder)
        {
            // Register Dapper type handlers once so every Dapper query in this module
            // can map SQL bit columns to BaseEntity.Status / BaseEntity.IsDelete enums.
            SqlMapper.AddTypeHandler(new StatusTypeHandler());
            SqlMapper.AddTypeHandler(new IsDeleteTypeHandler());

            var HangfireConnectionString = (configuration.GetConnectionString("HangfireConnection") ?? string.Empty)
                                               .Replace("{SERVER}", Environment.GetEnvironmentVariable("DATABASE_SERVER") ?? "")
                                               .Replace("{USER_ID}", Environment.GetEnvironmentVariable("DATABASE_USERID") ?? "")
                                               .Replace("{ENC_PASSWORD}", Environment.GetEnvironmentVariable("DATABASE_PASSWORD") ?? "");

            var NotificationConnectionString = (configuration.GetConnectionString("NotificationConnection") ?? string.Empty)
                                               .Replace("{SERVER}", Environment.GetEnvironmentVariable("DATABASE_SERVER") ?? "")
                                               .Replace("{USER_ID}", Environment.GetEnvironmentVariable("DATABASE_USERID") ?? "")
                                               .Replace("{ENC_PASSWORD}", Environment.GetEnvironmentVariable("DATABASE_PASSWORD") ?? "");

            if (string.IsNullOrWhiteSpace(HangfireConnectionString))
            {
                throw new InvalidOperationException("Connection string 'HangfireConnectionString' not found or is empty.");
            }
            else if (string.IsNullOrWhiteSpace(NotificationConnectionString))
            {
                throw new InvalidOperationException("Connection string 'NotificationConnectionString' not found or is empty.");
            }

            services.AddTransient<IHangfireDbConnectionFactory>(sp => new HangfireDbConnectionFactory(HangfireConnectionString));
            services.AddTransient<INotificationDbConnectionFactory>(sp => new NotificationDbConnectionFactory(NotificationConnectionString));
            // services.AddScoped<IDbConnection>(sp =>
            // {
            //     var factory = sp.GetRequiredService<INotificationDbConnectionFactory>();
            //     return factory.CreateConnection();
            // });
            services.AddKeyedScoped<IDbConnection>("Hangfire",
                (sp, key) => sp.GetRequiredService<IHangfireDbConnectionFactory>().CreateConnection());

            services.AddKeyedScoped<IDbConnection>("Notification",
              (sp, key) => sp.GetRequiredService<INotificationDbConnectionFactory>().CreateConnection());

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

            // Optional: Register IMongoDatabase if needed directly
            services.AddSingleton(sp =>
            {
                var mongoDbContext = (MongoDbContext)sp.GetRequiredService<IMongoDbContext>();
                return mongoDbContext.GetDatabase();
            });
            // Register Hangfire services
            services.AddHangfire(config =>
            {
                config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                      .UseSimpleAssemblyNameTypeSerializer()
                      .UseDefaultTypeSerializer()
                      .UseSqlServerStorage(HangfireConnectionString, new SqlServerStorageOptions
                      {
                          CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                          SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                          QueuePollInterval = TimeSpan.Zero,
                          UseRecommendedIsolationLevel = true,
                          UsePageLocksOnDequeue = true,
                          DisableGlobalLocks = true
                      });
            });

            services.AddDbContext<NotificationDbContext>(options =>
                options.UseSqlServer(NotificationConnectionString));

            // NOTE: AddHangfireServer() is NOT called here.
            // Only BSOFT.Worker registers a Hangfire server (in its Program.cs).
            // BSOFT.Api uses Hangfire storage for the dashboard and job enqueueing only.
            // MassTransit consumers are only registered when no other module has already called
            // AddMassTransit() — in BSOFT.Api, business modules register MassTransit first, so
            // this block is intentionally skipped (consumers run in BSOFT.Worker).
            // In BSOFT.Worker, this is the only AddMassTransit() call and runs in full.
            if (!services.Any(d => d.ServiceType == typeof(IBus)))
            {
                services.AddMassTransit(x =>
                {
                    x.SetKebabCaseEndpointNameFormatter();

                    // Notification Consumers - Single responsibility per channel
                    x.AddConsumer<NotificationDispatcherConsumer>();
                    x.AddConsumer<SendEmailNotificationConsumer>();
                    x.AddConsumer<SendSmsNotificationConsumer>();
                    x.AddConsumer<SendInAppNotificationConsumer>();
                    x.AddConsumer<SendWhatsappNotificationConsumer>();

                    // Workflow Consumers
                    x.AddConsumer<ApprovalRequestConsumer>();
                    x.AddConsumer<ApprovalResultDispatcherConsumer>();

                    // Module-specific approval result consumers
                    x.AddConsumer<PurchaseManagement.Application.Consumers.ApprovedRejectedConsumer>();
                    x.AddConsumer<BudgetManagement.Application.Consumers.ApprovedRejectedConsumer>();
                    x.AddConsumer<InventoryManagement.Application.Consumers.ApprovedRejectedConsumer>();
                    x.AddConsumer<PartyManagement.Application.Consumers.ApprovedRejectedConsumer>();

                    // Party → User integration consumers
                    x.AddConsumer<UserManagement.Application.Consumers.PartyApprovedConsumer>();
                    x.AddConsumer<UserManagement.Application.Consumers.PartySyncConsumer>();

                    x.AddConsumer<PurchaseManagement.Application.Consumers.RollbackTransactionConsumer>();
                    x.AddConsumer<BudgetManagement.Application.Consumers.RollbackTransactionConsumer>();
                    x.AddConsumer<PartyManagement.Application.Consumers.RollbackTransactionConsumer>();
                    x.AddConsumer<RollBackScheduleWorkOrderConsumer>();

                    x.UsingRabbitMq((context, cfg) =>
                    {
                        cfg.Host("localhost", "/", h =>
                        {
                            h.Username("guest");
                            h.Password("guest");
                        });

                        // ═══════════════════════════════════════════════════════════════════
                        // NOTIFICATION DISPATCHER - Entry point (resolves channels + routes)
                        // ═══════════════════════════════════════════════════════════════════
                        cfg.ReceiveEndpoint("notification-dispatcher-queue", e =>
                        {
                            e.PrefetchCount = 16;
                            e.UseMessageRetry(r => r
                                .Intervals(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30))
                                .Handle<Exception>());
                            e.ConfigureConsumer<NotificationDispatcherConsumer>(context);
                        });

                        // ═══════════════════════════════════════════════════════════════════
                        // CHANNEL-SPECIFIC QUEUES - With retry policies for external services
                        // ═══════════════════════════════════════════════════════════════════

                        // Email: Retry with exponential backoff (SMTP can have temp failures)
                        cfg.ReceiveEndpoint("email-notification-queue", e =>
                        {
                            e.PrefetchCount = 8;
                            e.UseMessageRetry(r => r
                                .Intervals(
                                    TimeSpan.FromSeconds(10),
                                    TimeSpan.FromSeconds(30),
                                    TimeSpan.FromMinutes(2),
                                    TimeSpan.FromMinutes(5))
                                .Handle<Exception>());
                            e.ConfigureConsumer<SendEmailNotificationConsumer>(context);
                        });

                        // SMS: Retry for API failures
                        cfg.ReceiveEndpoint("sms-notification-queue", e =>
                        {
                            e.PrefetchCount = 8;
                            e.UseMessageRetry(r => r
                                .Intervals(
                                    TimeSpan.FromSeconds(10),
                                    TimeSpan.FromSeconds(30),
                                    TimeSpan.FromMinutes(2))
                                .Handle<Exception>());
                            e.ConfigureConsumer<SendSmsNotificationConsumer>(context);
                        });

                        // InApp: Fast local processing, minimal retry
                        cfg.ReceiveEndpoint("inapp-notification-queue", e =>
                        {
                            e.PrefetchCount = 16;
                            e.UseMessageRetry(r => r.Immediate(3));
                            e.ConfigureConsumer<SendInAppNotificationConsumer>(context);
                        });

                        // WhatsApp: Retry for API failures
                        cfg.ReceiveEndpoint("whatsapp-notification-queue", e =>
                        {
                            e.PrefetchCount = 8;
                            e.UseMessageRetry(r => r
                                .Intervals(
                                    TimeSpan.FromSeconds(10),
                                    TimeSpan.FromSeconds(30),
                                    TimeSpan.FromMinutes(2))
                                .Handle<Exception>());
                            e.ConfigureConsumer<SendWhatsappNotificationConsumer>(context);
                        });
                        cfg.ReceiveEndpoint("approval-request-task-queue", e =>
                        {
                            e.ConfigureConsumer<ApprovalRequestConsumer>(context);
                        });

                        // Approval result dispatcher — routes ApprovedRejectedEvent to module queues
                        cfg.ReceiveEndpoint("approval-result-dispatcher-queue", e =>
                        {
                            e.UseMessageRetry(r => r.Intervals(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30)));
                            e.ConfigureConsumer<ApprovalResultDispatcherConsumer>(context);
                        });

                        // Module-specific approval result queues
                        cfg.ReceiveEndpoint("approved-rejected-purchase-task-queue", e =>
                        {
                            e.UseMessageRetry(r => r.Intervals(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30)));
                            e.ConfigureConsumer<PurchaseManagement.Application.Consumers.ApprovedRejectedConsumer>(context);
                        });
                        cfg.ReceiveEndpoint("approved-rejected-budget-task-queue", e =>
                        {
                            e.UseMessageRetry(r => r.Intervals(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30)));
                            e.ConfigureConsumer<BudgetManagement.Application.Consumers.ApprovedRejectedConsumer>(context);
                        });
                        cfg.ReceiveEndpoint("approved-rejected-inventory-task-queue", e =>
                        {
                            e.UseMessageRetry(r => r.Intervals(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30)));
                            e.ConfigureConsumer<InventoryManagement.Application.Consumers.ApprovedRejectedConsumer>(context);
                        });
                        cfg.ReceiveEndpoint("approved-rejected-party-task-queue", e =>
                        {
                            e.UseMessageRetry(r => r.Intervals(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30)));
                            e.ConfigureConsumer<PartyManagement.Application.Consumers.ApprovedRejectedConsumer>(context);
                        });

                        // Rollback queues (undo pending status on workflow failure)
                        cfg.ReceiveEndpoint("approval-request-rollback-purchase-queue", e =>
                        {
                            e.ConfigureConsumer<PurchaseManagement.Application.Consumers.RollbackTransactionConsumer>(context);
                        });
                        cfg.ReceiveEndpoint("approval-request-rollback-budget-queue", e =>
                        {
                            e.ConfigureConsumer<BudgetManagement.Application.Consumers.RollbackTransactionConsumer>(context);
                        });
                        cfg.ReceiveEndpoint("approval-request-rollback-party-queue", e =>
                        {
                            e.ConfigureConsumer<PartyManagement.Application.Consumers.RollbackTransactionConsumer>(context);
                        });

                        cfg.ReceiveEndpoint("rollback-ScheduleWorkOrder-queue", e =>
                        {
                            e.ConfigureConsumer<RollBackScheduleWorkOrderConsumer>(context);
                        });
                    });
                });
            }
 
            // ✅ Correctly bind EmailSettings
            var emailSettings = new EmailSettings();
            configuration.GetSection("EmailSettings").Bind(emailSettings);
            services.AddSingleton(emailSettings);

            var smsSettings = new SmsSettings();
            configuration.GetSection("SmsSettings").Bind(smsSettings);
            services.AddSingleton(smsSettings);

            // ✅ WhatsApp settings
            var whatsAppSettings = new WhatsAppSettings();
            configuration.GetSection("WhatsAppSettings").Bind(whatsAppSettings);
            services.AddSingleton(whatsAppSettings);

            services.AddHttpClient("UserManagementClient", client =>
           {
               //client.BaseAddress = new Uri("http://localhost:5174"); 

               client.BaseAddress = new Uri(configuration["HttpClientSettings:UserManagementService"]);          
           })

              .AddTransientHttpErrorPolicy(policyBuilder =>
               policyBuilder.CircuitBreakerAsync(
                   handledEventsAllowedBeforeBreaking: 3,
                   durationOfBreak: TimeSpan.FromSeconds(30)))
           .AddTransientHttpErrorPolicy(policyBuilder =>
               policyBuilder.WaitAndRetryAsync(3, retryAttempt =>
                   TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));

            services.AddHttpClient("MaintenanceClient", client =>
           {
               //client.BaseAddress = new Uri("http://localhost:5174"); 
               client.BaseAddress = new Uri(configuration["HttpClientSettings:MaintenanceManagementService"]);

           })

              .AddTransientHttpErrorPolicy(policyBuilder =>
               policyBuilder.CircuitBreakerAsync(
                   handledEventsAllowedBeforeBreaking: 3,
                   durationOfBreak: TimeSpan.FromSeconds(30)))
           .AddTransientHttpErrorPolicy(policyBuilder =>
               policyBuilder.WaitAndRetryAsync(3, retryAttempt =>
                   TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));


            services.AddAutoMapper(typeof(NotificationHierarchyAndEventRuleProfile));

            services.AddHttpClient();
            services.AddScoped<IEmailService, RealEmailService>();
            services.AddScoped<ISmsService, RealSmsService>();
            services.AddScoped<IUserUnlockService, UserUnlockService>();
            services.AddScoped<IUserUnlockBackgroundJob, UserUnlockBackgroundJob>();
            services.AddScoped<INotificationConfigCommandRepository, NotificationConfigCommandRepository>();  
            services.AddScoped<INotificationConfigQueryRepository, NotificationConfigQueryRepository>();  
            services.AddScoped<INotificationGroupCommand, NotificationGroupCommandRepository >();
            services.AddScoped<INotificationGroupQuery, NotificationGroupQueryRepository >();
            services.AddScoped<INotificationWhatsAppGroupCommand, NotificationWhatsAppGroupCommandRepository>();
            services.AddScoped<INotificationWhatsAppGroupQuery, NotificationWhatsAppGroupQueryRepository>();
            services.AddScoped<ILookupRepository, LookupRepository>();
            services.AddSingleton<ITimeZoneService, TimeZoneService>();


            services.AddTransient<IJwtTokenHelper, JwtTokenHelper>();         
            services.AddScoped<INotificationTemplateCommandRepository, NotificationTemplateCommandRepository>();  
            services.AddScoped<INotificationTemplateQueryRepository, NotificationTemplateQueryRepository>();
            services.AddScoped<INotificationUserResolver, NotificationUserResolver>();
            services.AddScoped<INotificationDetailRepository, NotificationDetailRepository>();
            
            //services.AddScoped<INotificationResolverHandler, NotificationResolverHandler>();
			services.AddScoped<IMiscMasterCommandRepository, MiscMasterCommandRepository>();
            services.AddScoped<IMiscMasterQueryRepository, MiscMasterQueryRepository>();
            services.AddScoped<IMiscTypeMasterCommandRepository , MiscTypeMasterCommandRepository>();
            services.AddScoped<IMiscTypeMasterQueryRepository , MiscTypeMasterQueryRepository>();
            //Notification
            services.AddScoped<IEmailSender, EmailSender>();
            services.AddScoped<ISmsSender, SmsSender>();
            services.AddScoped<IInAppNotifier, InAppNotifier>(); 
            services.AddScoped<IWhatsAppSender, WhatsAppSender>();
            services.AddScoped<INotificationGroupMemberCommand, NotificationGroupMemberCommandRepository >();
            services.AddScoped<INotificationGroupMemberQuery, NotificationGroupMemberQueryRepository >();

            services.AddScoped<INotificationLevelHierarchyCommand, NotificationLevelHierarchyCommand>();
            services.AddScoped<INotificationEventRuleCommand, NotificationEventRuleCommand>();
            services.AddScoped<INotificationLogger, NotificationLogger>();
            
            services.AddScoped<IWorkflowLookup, WorkflowLookupRepository>();
            services.AddScoped<IWorkflowTypeQuery, WorkflowTypeQueryRepository >();
            services.AddScoped<IWorkflowTypeCommand, WorkflowTypeCommandRepository >();
             services.AddScoped<IApprovalStepDetailQuery, ApprovalStepDetailQueryRepository >();
            services.AddScoped<IApprovalStepDetailCommand, ApprovalStepDetailCommandRepository >();
             services.AddScoped<IApprovalRuleQuery, ApprovalRuleQueryRepository >();
            services.AddScoped<IApprovalRuleCommand, ApprovalRuleCommandRepository >();
            services.AddScoped<IApprovalRequestQuery, ApprovalRequestQueryRepository >();
            services.AddScoped<IApprovalRequestCommand, ApprovalRequestCommandRepository >();
            services.AddScoped<IEventPublisher, EventPublisher>();
            services.AddScoped<INotificationResolverHandler, NotificationResolverHandler>();
            services.AddScoped<INotificationTablePresetRepository, NotificationTablePresetRepository>();
            services.AddScoped<IHtmlTableRenderer, SqlHtmlTableRenderer>();          
            
            services.AddHttpClient<IFileFetcher, HttpFileFetcher>();              

            services.AddScoped<IMongoCollection<OutboxMessage>>(sp =>
            {   
                var database = sp.GetRequiredService<IMongoDatabase>();
                var collectionName = configuration["MongoDbSettings:OutboxCollectionName"] ?? "OutboxMessages";
                return database.GetCollection<OutboxMessage>(collectionName);
            });
            services.AddScoped<IFileStorageService, FileStorageService>();
            services.AddScoped<IHangfireQuery, HangfireQueryRepository>();
            services.AddScoped<SqlOutboxProcessorJob>();

            // Inbox/Dedup — prevents duplicate message processing on MassTransit redelivery
            services.AddScoped<IInboxRepository, InboxRepository>();

            return services;
        }
    }
}

