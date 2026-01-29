

// using Contracts.Commands.Maintenance;
// using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
// using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
// using MassTransit;
// using Microsoft.Extensions.Logging;

// namespace MaintenanceManagement.Application.Consumers
// {
//     public class RollbackWorkOrderConsumer : IConsumer<RollbackWorkOrderCommand>
//     {
//         private readonly IWorkOrderCommandRepository _workOrderRepo;
//         private readonly ILogger<RollbackWorkOrderConsumer> _logger;
//         private readonly IPreventiveSchedulerCommand _preventiveSchedulerCommand;

//         public RollbackWorkOrderConsumer(IWorkOrderCommandRepository repo, ILogger<RollbackWorkOrderConsumer> logger, IPreventiveSchedulerCommand preventiveSchedulerCommand)
//         {
//             _workOrderRepo = repo;
//             _logger = logger;
//             _preventiveSchedulerCommand = preventiveSchedulerCommand;
//         }

//         public async Task Consume(ConsumeContext<RollbackWorkOrderCommand> context)
//         {
//             _logger.LogInformation("⚠️ Rollback requested for WorkOrder ConsumeContext: {@context} ",
//                 context.Message);

//             // Rollback logic: for example, reset the WorkOrder status
//             var result = await _workOrderRepo.RevertWorkOrderStatusAsync(context.Message.WorkOrderId);
//             await _preventiveSchedulerCommand.DeleteDetailByDetailId(context.Message.SchedulerId);

//             if (!result)
//             {
//                 _logger.LogError("❌ Failed to revert work order status for ConsumeContext: {@context}", context.Message);
//             }
//         }
//     }
// }