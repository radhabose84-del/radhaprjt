// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Contracts.Commands.Party;
// using PartyManagement.Application.Common.Interfaces.IPartyMaster;
// using MassTransit;

// namespace PartyManagement.Application.Consumers
// {
//     public class RollbackTransactionConsumer : IConsumer<RollbackTransactionCommand>
//     {
//          private readonly IPartyMasterCommandRepository _partyMasterCommandRepository;
//         public RollbackTransactionConsumer(IPartyMasterCommandRepository partyMasterCommandRepository)
//         {
//             _partyMasterCommandRepository = partyMasterCommandRepository;
//         }
//         public async Task Consume(ConsumeContext<RollbackTransactionCommand> context)
//         {
//             await _partyMasterCommandRepository.RollbackStatusAsync(context.Message.ModuleTransactionId);
//         }
//     }
// }