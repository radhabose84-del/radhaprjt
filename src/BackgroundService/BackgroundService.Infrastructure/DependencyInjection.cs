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
using BackgroundService.Application.Consumer.PreventiveSchedule.Update;
using BackgroundService.Application.Interfaces.Files;
using BackgroundService.Infrastructure.Files;



namespace BackgroundService.Infrastructure
{
    public static class DependencyInjection
    {
        private static readonly string[] HangfireQueues = ["schedule_work_order_queue","forgot_password_queue","user_unlock_queue"];

        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration, IServiceCollection builder)
        {
            var HangfireConnectionString = configuration.GetConnectionString("HangfireConnection")
                                               .Replace("{SERVER}", Environment.GetEnvironmentVariable("DATABASE_SERVER") ?? "")
                                               .Replace("{USER_ID}", Environment.GetEnvironmentVariable("DATABASE_USERID") ?? "")
                                               .Replace("{ENC_PASSWORD}", Environment.GetEnvironmentVariable("DATABASE_PASSWORD") ?? "");

             var NotificationConnectionString = configuration.GetConnectionString("NotificationConnection")
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

            // Add the Hangfire server
            services.AddHangfireServer(options =>
            {
                options.ServerName = configuration["HangfireServer:Server"];
                options.Queues = HangfireQueues;
            });
            //Notification
            services.AddMassTransit(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();
                x.AddConsumer<ResolveNotificationChannelsConsumer>();
                x.AddConsumer<SendEmailNotificationConsumer>();
                x.AddConsumer<SendSmsNotificationConsumer>();
                x.AddConsumer<SendInAppNotificationConsumer>();
                x.AddConsumer<ApprovalRequestConsumer>();
                x.AddConsumer<ScheduleWorkOrderConsumer>();
                x.AddConsumer<NewScheduleWorkOrderTaskConsumer>();
                x.AddConsumer<RollBackScheduleWorkOrderConsumer>();
                x.AddConsumer<ScheduleWorkOrderUpdateConsumer>();
                x.AddConsumer<SendWhatsappNotificationConsumer>();
                

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host("localhost", "/", h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });
                    cfg.ReceiveEndpoint("resolve-notification-channels-queue", e =>
                    {          
                        e.ConfigureConsumer<ResolveNotificationChannelsConsumer>(context);
                    });
                    cfg.ReceiveEndpoint("email-notification-queue", e =>
                    {
                        e.ConfigureConsumer<SendEmailNotificationConsumer>(context);
                    });
                    cfg.ReceiveEndpoint("sms-notification-queue", e =>
                    {                        
                        e.ConfigureConsumer<SendSmsNotificationConsumer>(context);
                    });
                    cfg.ReceiveEndpoint("inapp-notification-queue", e =>
                    {                       
                        e.ConfigureConsumer<SendInAppNotificationConsumer>(context);
                    });
                    cfg.ReceiveEndpoint("whatsapp-notification-queue", e =>
                    {
                        e.ConfigureConsumer<SendWhatsappNotificationConsumer>(context);
                    });
                    cfg.ReceiveEndpoint("approval-request-task-queue", e =>
                    {
                        e.ConfigureConsumer<ApprovalRequestConsumer>(context);
                    });
                    
                    cfg.ReceiveEndpoint("hangfire-workorder-schedule-queue", e =>
                    {
                        e.ConfigureConsumer<ScheduleWorkOrderConsumer>(context);
                    });
                     cfg.ReceiveEndpoint("schedule-workorder-queue", e =>
                    {
                        e.ConfigureConsumer<NewScheduleWorkOrderTaskConsumer>(context);
                    });
                    cfg.ReceiveEndpoint("rollback-ScheduleWorkOrder-queue", e =>
                    {
                        e.ConfigureConsumer<RollBackScheduleWorkOrderConsumer>(context);
                    });
                     cfg.ReceiveEndpoint("update-scheduleWorkOrder-task-queue", e =>
                    {
                        e.ConfigureConsumer<ScheduleWorkOrderUpdateConsumer>(context);
                    });
                    //cfg.ConfigureEndpoints(context);
                     
                });
            });
 
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
            services.AddTransient<IVerificationCodeCleanupService, VerificationCodeCleanupService>();
            services.AddScoped<IUserUnlockBackgroundJob, UserUnlockBackgroundJob>();
            services.AddTransient<IMaintenance, MaintenanceService>();
            services.AddScoped<INotificationConfigCommandRepository, NotificationConfigCommandRepository>();  
            services.AddScoped<INotificationConfigQueryRepository, NotificationConfigQueryRepository>();  
            services.AddScoped<INotificationGroupCommand, NotificationGroupCommandRepository >();
            services.AddScoped<INotificationGroupQuery, NotificationGroupQueryRepository >();
            services.AddScoped<INotificationWhatsAppGroupCommand, NotificationWhatsAppGroupCommandRepository>();
            services.AddScoped<INotificationWhatsAppGroupQuery, NotificationWhatsAppGroupQueryRepository>();
            services.AddScoped<IIPAddressService, IPAddressService>();

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
            

             services.AddScoped<IWorkflowTypeQuery, WorkflowTypeQueryRepository >();
            services.AddScoped<IWorkflowTypeCommand, WorkflowTypeCommandRepository >();
             services.AddScoped<IApprovalStepDetailQuery, ApprovalStepDetailQueryRepository >();
            services.AddScoped<IApprovalStepDetailCommand, ApprovalStepDetailCommandRepository >();
             services.AddScoped<IApprovalRuleQuery, ApprovalRuleQueryRepository >();
            services.AddScoped<IApprovalRuleCommand, ApprovalRuleCommandRepository >();
            services.AddScoped<IApprovalRequestQuery, ApprovalRequestQueryRepository >();
            services.AddScoped<IApprovalRequestCommand, ApprovalRequestCommandRepository >();
            services.AddScoped<IEventPublisher, EventPublisher>();
            services.AddScoped<IApprovalRequestGrpcQuery, ApprovalRequestQueryRepository >();
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
            return services;
        }
    }
}

