// using Contracts.Events.Users;
// using Core.Application.Common.Interfaces;
// using MassTransit;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.DependencyInjection;
// using MongoDB.Driver;
// using UserManagement.Infrastructure.Persistence;
// using UserManagement.Infrastructure.Services;

// namespace UserManagement.Infrastructure
// {
//     public static class SagaDependencyInjection
//     {
//         public static IServiceCollection AddSagaInfrastructure(this IServiceCollection services, IConfiguration configuration)
//         {
//             // Mongo outbox store
//             services.AddScoped<IMongoCollection<OutboxMessage>>(sp =>
//             {
//                 var database = sp.GetRequiredService<IMongoDatabase>();
//                 var collectionName = configuration["MongoDbSettings:OutboxCollectionName"] ?? "OutboxMessages";
//                 return database.GetCollection<OutboxMessage>(collectionName);
//             });

//             services.AddScoped<IEventPublisher, EventPublisher>();

//             services.AddMassTransit(x =>
//             {

//                  x.SetKebabCaseEndpointNameFormatter();
//                 // Register consumers
//                 x.AddConsumer<Core.Application.Consumers.PartyApprovedConsumer>();
//                 x.AddConsumer<Core.Application.Consumers.PartySyncConsumer>();

//                 x.UsingRabbitMq((context, cfg) =>
//                 {
//                     cfg.Host("localhost", "/", h =>
//                     {
//                         h.Username("guest");
//                         h.Password("guest");
//                     });

//                       // (Optional) JSON serializer tweaks
//                     cfg.ConfigureJsonSerializerOptions(opts =>
//                     {
//                         opts.PropertyNameCaseInsensitive = true;
//                         return opts;
//                     });

//                     // If you want consistent, readable endpoint names across the app:
//                     // cfg.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter(false));

//                     // PartyApproved endpoint
//                     cfg.ReceiveEndpoint("party-approved", e =>
//                     {

//                          // we will bind manually to the event exchange below
//                         e.ConfigureConsumeTopology = false;

//                         e.PrefetchCount = 16;
//                         e.ConcurrentMessageLimit = 8;
//                         // Quick retries for transient faults
//                         e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));

//                         // transient faults
//                         e.UseDelayedRedelivery(r => r.Intervals(
//                             TimeSpan.FromSeconds(30),
//                             TimeSpan.FromMinutes(2),
//                             TimeSpan.FromMinutes(10)));

//                         // outbox to prevent duplicate side-effects inside the consumer
//                         e.UseInMemoryOutbox();

//                          // 👇 ensure routing from the published event exchange to this queue
//                         e.Bind<PartyApprovedIntegrationEvent>();

//                         // Serialize messages per PartyId to avoid races for the same party
//                         e.UsePartitioner(8, ctx =>
//                             ctx.TryGetMessage<PartyApprovedIntegrationEvent>(out var m)
//                                 ? m.Message.PartyId
//                                 : 0);

//                         e.ConfigureConsumer<Core.Application.Consumers.PartyApprovedConsumer>(context);
//                     });

//                     // PartySync endpoint
//                     cfg.ReceiveEndpoint("party-sync", e =>
//                     {

//                         e.ConfigureConsumeTopology = false;

//                         e.PrefetchCount = 16;
//                         e.ConcurrentMessageLimit = 8;
//                          e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
//                         e.UseDelayedRedelivery(r => r.Intervals(
//                             TimeSpan.FromSeconds(30),
//                             TimeSpan.FromMinutes(2),
//                             TimeSpan.FromMinutes(10)));

//                         e.UseInMemoryOutbox();

//                          // 👇 bind to the published event exchange
//                         e.Bind<PartySyncIntegrationEvent>();

//                         e.UsePartitioner(8, ctx =>
//                             ctx.TryGetMessage<PartySyncIntegrationEvent>(out var m)
//                                 ? m.Message.PartyId
//                                 : 0);

//                         e.ConfigureConsumer<Core.Application.Consumers.PartySyncConsumer>(context);
//                     });
//                 });
//             });

//             // NOTE: Do NOT add AddMassTransitHostedService() with the new container integration.
//             // It’s already handled by AddMassTransit.

//             return services;
//         }
//     }
// }