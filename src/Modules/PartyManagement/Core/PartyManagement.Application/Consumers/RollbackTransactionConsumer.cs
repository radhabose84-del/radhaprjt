using Contracts.Commands.Party;
using PartyManagement.Application.Common.Interfaces.IPartyMaster;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace PartyManagement.Application.Consumers
{
    public class RollbackTransactionConsumer : IConsumer<RollbackTransactionCommand>
    {
        private readonly IPartyMasterCommandRepository _partyMasterCommandRepository;
        private readonly ILogger<RollbackTransactionConsumer> _logger;

        public RollbackTransactionConsumer(
            IPartyMasterCommandRepository partyMasterCommandRepository,
            ILogger<RollbackTransactionConsumer> logger)
        {
            _partyMasterCommandRepository = partyMasterCommandRepository;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<RollbackTransactionCommand> context)
        {
            var msg = context.Message;
            _logger.LogWarning(
                "Rolling back Party {PartyId}. Reason: {Reason}",
                msg.ModuleTransactionId, msg.Reason);

            await _partyMasterCommandRepository.RollbackStatusAsync(msg.ModuleTransactionId);
        }
    }
}
