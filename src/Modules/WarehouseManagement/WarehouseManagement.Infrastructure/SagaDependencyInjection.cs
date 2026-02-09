using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WarehouseManagement.Infrastructure
{
    public static class SagaDependencyInjection
    {
        public static IServiceCollection AddSagaInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {

            // // Register the OutboxMessage collection
            // services.AddScoped<IMongoCollection<OutboxMessage>>(sp =>
            // {
            //     var database = sp.GetRequiredService<IMongoDatabase>();
            //     var collectionName = configuration["MongoDbSettings:OutboxCollectionName"] ?? "OutboxMessages";
            //     return database.GetCollection<OutboxMessage>(collectionName);
            // });

            // // Register your EventPublisher service
            // services.AddScoped<IEventPublisher, EventPublisher>();
            // // Configure MassTransit with RabbitMQ
            // services.AddMassTransit(x =>
            // {
            //     x.UsingRabbitMq((context, cfg) =>
            //     {
            //         cfg.Host("localhost", "/", h =>
            //         {
            //             h.Username("guest");
            //             h.Password("guest");
            //         });
            //     });
            // });

            // // Ensure MassTransit background service is added
            // services.AddMassTransitHostedService();

            return services;
        }
    }
}