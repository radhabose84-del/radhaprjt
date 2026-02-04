// using InventoryManagement.Application.Common.Interfaces;
// using InventoryManagement.Application.Consumers;
// using InventoryManagement.Infrastructure.Persistence;
// using InventoryManagement.Infrastructure.Services;
// using MassTransit;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.DependencyInjection;
// using MongoDB.Driver;

// namespace InventoryManagement.Infrastructure
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
//                 x.AddConsumer<ApprovedRejectedConsumer>();
                
//                 x.UsingRabbitMq((context, cfg) =>
//                 {
//                     cfg.Host("localhost", "/", h =>
//                     {
//                         h.Username("guest");
//                         h.Password("guest");
//                     });
 

//                     cfg.ReceiveEndpoint("approved-rejected-inventory-task-queue", e =>
//                    {
//                        e.ConfigureConsumer<ApprovedRejectedConsumer>(context);
//                    });
                    
//                 });

                
//             });

//             // Ensure MassTransit background service is added
//             services.AddMassTransitHostedService();

//             return services;
//         }
//     }
// }