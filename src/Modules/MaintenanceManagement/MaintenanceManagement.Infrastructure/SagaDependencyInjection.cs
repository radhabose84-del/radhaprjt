// using MaintenanceManagement.Application.Common.Interfaces;
// using MaintenanceManagement.Application.Consumers;
// using MaintenanceManagement.Application.Consumers.PreventiveScheduler;
// using MaintenanceManagement.Infrastructure.Persistence;
// using MaintenanceManagement.Infrastructure.Services;
// using MassTransit;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.DependencyInjection;
// using MongoDB.Driver;
// namespace MaintenanceManagement.Infrastructure
// {
//     public static class SagaDependencyInjection
//     {
//         public static IServiceCollection AddSagaInfrastructure(this IServiceCollection services, IConfiguration configuration)
//         {
//             // Register the OutboxMessage collection
//             services.AddScoped<IMongoCollection<OutboxMessage>>(sp =>
//             {
//                 var database = sp.GetRequiredService<IMongoDatabase>();
//                 var collectionName = configuration["MongoDbSettings:OutboxCollectionName"] ?? "OutboxMessages";
//                 return database.GetCollection<OutboxMessage>(collectionName);
//             });

//             // Register your EventPublisher service
//             services.AddScoped<IEventPublisher, EventPublisher>();

//             // Configure MassTransit with RabbitMQ
//             services.AddMassTransit(x =>
//             {
//                 x.SetKebabCaseEndpointNameFormatter();

//                 // Register Consumer
//                 x.AddConsumer<ScheduleNextPreventiveTaskConsumer>();
//                 x.AddConsumer<RollbackWorkOrderConsumer>();
//                 x.AddConsumer<ScheduleDetailCreateTaskConsumer>();
//                 // x.AddConsumer<ScheduleWorkOrderTaskConsumer>();
//                 x.AddConsumer<RollbackPreventiveDetailConsumer>();
//                 // x.AddConsumer<RollBackScheduleWorkOrderConsumer>();
//                 // x.AddConsumer<MaintenanceManagement.Application.Consumers.PreventiveScheduler.Update.ScheduleWorkOrderConsumer>();
//                 x.AddConsumer<MaintenanceManagement.Application.Consumers.PreventiveScheduler.Update.RollBackScheduleWorkOrderConsumer>();
//                 // x.AddConsumer<RollbackNextScheduleConsumer>();

//                 x.UsingRabbitMq((context, cfg) =>
//                 {
//                     cfg.Host("localhost", "/", h =>
//                     {
//                         h.Username("guest");
//                         h.Password("guest");
//                     });

//                     // ✅ Named consumer queue
//                     cfg.ReceiveEndpoint("schedule-next-task-queue", e =>
//                     {
//                         e.ConfigureConsumer<ScheduleNextPreventiveTaskConsumer>(context);
//                     });
//                     cfg.ReceiveEndpoint("rollback-workorder-queue", e =>
//                     {
//                         e.ConfigureConsumer<RollbackWorkOrderConsumer>(context);
//                     });
//                     cfg.ReceiveEndpoint("schedule-detail-task-queue", e =>
//                    {
//                        e.ConfigureConsumer<ScheduleDetailCreateTaskConsumer>(context);
//                    });
//                     // cfg.ReceiveEndpoint("schedule-workorder-queue", e =>
//                     // {
//                     //     e.ConfigureConsumer<ScheduleWorkOrderTaskConsumer>(context);
//                     // });
//                     cfg.ReceiveEndpoint("rollback-scheduleHeader-queue", e =>
//                     {
//                         e.ConfigureConsumer<RollbackPreventiveDetailConsumer>(context);
//                     });

//                     // cfg.ReceiveEndpoint("rollback-ScheduleWorkOrder-queue", e =>
//                     // {
//                     //     e.ConfigureConsumer<RollBackScheduleWorkOrderConsumer>(context);
//                     // });
//                     cfg.ReceiveEndpoint("update-rollback-scheduleWorkOrder-queue", e =>
//                 {
//                     e.ConfigureConsumer<MaintenanceManagement.Application.Consumers.PreventiveScheduler.Update.RollBackScheduleWorkOrderConsumer>(context);
//                 });

//                     // cfg.ReceiveEndpoint("update-scheduleWorkOrder-task-queue", e =>
//                     // {
//                     //     e.ConfigureConsumer<MaintenanceManagement.Application.Consumers.PreventiveScheduler.Update.ScheduleWorkOrderConsumer>(context);
//                     // });

//                     //  cfg.ReceiveEndpoint("hangfire-next-schedule-queue", e =>
//                     // {
//                     //     e.ConfigureConsumer<RollbackNextScheduleConsumer>(context);
//                     // });


//                 });
//                  services.AddMassTransitHostedService();
//             });
//             return services;
//         }
//     }
// }