using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournalFlag;
using FinanceManagement.Domain.Entities;
using FinanceManagement.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FinanceManagement.Application.JournalMaster.JournalThresholdRule.EventHandlers
{
    // US-GL01-16B — evaluates the active 16A threshold rules against a just-posted journal and raises
    // JournalFlag rows. Alert-only: failures here must NEVER block or roll back the posting, so the
    // whole body is guarded and any error is logged and swallowed.
    public class JournalPostedFlaggingEventHandler : INotificationHandler<JournalPostedDomainEvent>
    {
        private readonly IJournalFlagEngineRepository _repository;
        private readonly ITimeZoneService _timeZoneService;
        private readonly ILogger<JournalPostedFlaggingEventHandler> _logger;

        public JournalPostedFlaggingEventHandler(
            IJournalFlagEngineRepository repository,
            ITimeZoneService timeZoneService,
            ILogger<JournalPostedFlaggingEventHandler> logger)
        {
            _repository = repository;
            _timeZoneService = timeZoneService;
            _logger = logger;
        }

        public async Task Handle(JournalPostedDomainEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                var rules = await _repository.GetActiveThresholdRulesAsync(cancellationToken);
                if (rules.Count == 0)
                    return;

                var now = _timeZoneService.GetCurrentTime();
                var flags = new List<JournalFlag>();

                foreach (var rule in rules)
                {
                    if (Breaches(rule, notification))
                    {
                        flags.Add(new JournalFlag
                        {
                            JournalHeaderId = notification.JournalId,
                            RuleTypeId = rule.RuleTypeId,
                            Value = notification.Amount,
                            FlaggedAt = now,
                            DigestSent = false
                        });
                    }
                }

                if (flags.Count > 0)
                    await _repository.AddFlagsAsync(flags, cancellationToken);
            }
            catch (Exception ex)
            {
                // Flagging is advisory — never let it affect the (already committed) posting.
                _logger.LogError(ex, "Flagging engine failed for posted journal {JournalId}.", notification.JournalId);
            }
        }

        // Extensible rule evaluation. Rules needing extra context (control-account manual, closed-period
        // override) are not evaluable from the posting event alone and are intentionally skipped here.
        private static bool Breaches(ActiveThresholdRule rule, JournalPostedDomainEvent e) => rule.RuleTypeCode switch
        {
            "AMT_OVER" => rule.ThresholdValue.HasValue && e.Amount > rule.ThresholdValue.Value,
            "ROUND_NUM" => e.Amount > 0 && e.Amount % 100000m == 0,
            "WEEKEND_POST" => e.PostingDate.HasValue &&
                              (e.PostingDate.Value.DayOfWeek == DayOfWeek.Saturday || e.PostingDate.Value.DayOfWeek == DayOfWeek.Sunday),
            "SAME_DAY_REV" => e.IsReversal,
            _ => false
        };
    }
}
