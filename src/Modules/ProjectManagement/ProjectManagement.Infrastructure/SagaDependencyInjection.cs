// using ProjectManagement.Application.Common.Interfaces;
// using ProjectManagement.Infrastructure.Persistence;
// using ProjectManagement.Infrastructure.Services;
// using MassTransit;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.DependencyInjection;
// using MongoDB.Driver;
// using ProjectManagement.Application.Consumers;
// namespace ProjectManagement.Infrastructure
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
//           services.AddMassTransit(x =>
//             {
//                 x.SetKebabCaseEndpointNameFormatter();
//                 x.AddConsumer<ProjectApprovedRejectedConsumer>();
//               //  x.AddConsumer<RollbackTransactionConsumer>();

//                 x.UsingRabbitMq((context, cfg) =>
//                 {
//                     cfg.Host("localhost", "/", h =>
//                     {
//                         h.Username("guest");
//                         h.Password("guest");
//                     });
 

//                     cfg.ReceiveEndpoint("approved-rejected-project-task-queue", e =>
//                    {
//                        e.ConfigureConsumer<ProjectApprovedRejectedConsumer>(context);
//                    });
//                     //   cfg.ReceiveEndpoint("approval-request-rollback-queue", e =>
//                     // {
//                     //     e.ConfigureConsumer<RollbackTransactionConsumer>(context);
//                     // });
//                 });

                
//             });
//                  services.AddMassTransitHostedService();
          
//             return services;
//         }
//     }
// }