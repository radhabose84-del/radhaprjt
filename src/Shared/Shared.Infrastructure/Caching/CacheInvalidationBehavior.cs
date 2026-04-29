#nullable enable
using MediatR;
using Microsoft.Extensions.Logging;

namespace Shared.Infrastructure.Caching;

/// <summary>
/// MediatR pipeline behavior that evicts cached lookup entries after a successful
/// Create / Update / Delete command. Convention: command class name "Create{Entity}Command",
/// "Update{Entity}Command", or "Delete{Entity}Command" maps to lookup interface "I{Entity}Lookup".
///
/// Safety:
///  • Eviction runs ONLY after the inner handler returns successfully. Exceptions skip eviction.
///  • Excluded lookups (e.g. IWorkflowLookup, IDocumentSequenceLookup) are skipped — they are
///    never cached in the first place.
///  • Lookups not registered as cached are silently skipped (no error).
///  • Any exception during eviction is logged and swallowed — eviction failure must NEVER
///    roll back a successful command response.
/// </summary>
public sealed class CacheInvalidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private const string CreatePrefix = "Create";
    private const string UpdatePrefix = "Update";
    private const string DeletePrefix = "Delete";
    private const string CommandSuffix = "Command";

    private readonly LookupCacheInvalidator _invalidator;
    private readonly ILogger<CacheInvalidationBehavior<TRequest, TResponse>> _logger;

    public CacheInvalidationBehavior(
        LookupCacheInvalidator invalidator,
        ILogger<CacheInvalidationBehavior<TRequest, TResponse>> logger)
    {
        _invalidator = invalidator;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // 1. Run the inner handler first. If it throws (validation, business rule, DB),
        //    the exception propagates and eviction is never attempted.
        var response = await next();

        // 2. Successful command — decide whether to evict.
        var commandName = typeof(TRequest).Name;
        if (!TryGetLookupInterfaceName(commandName, out var lookupName))
            return response;

        if (CachingServiceExtensions.ExcludedLookupInterfaces.Contains(lookupName))
            return response;

        if (!CachingServiceExtensions.RegisteredCachedLookupNames.Contains(lookupName))
            return response;

        // 3. Evict — wrapped in try/catch so eviction failure cannot break the response.
        try
        {
            _invalidator.Evict(lookupName);
            _logger.LogDebug(
                "Lookup cache evicted for {Lookup} after {Command}",
                lookupName,
                commandName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Lookup cache eviction failed for {Lookup} after {Command}; cache will refresh on TTL",
                lookupName,
                commandName);
        }

        return response;
    }

    /// <summary>
    /// Convention parser:
    ///   "CreateItemCommand"                          → "IItemLookup"
    ///   "UpdateCustomerCommand"                      → "ICustomerLookup"
    ///   "DeleteAgentCommand"                         → "IAgentLookup"
    ///   "CreateAgentCommissionConfigurationCommand"  → "IAgentCommissionConfigurationLookup"
    ///   "ApproveOrderCommand"                        → no match (returns false)
    ///   "GetItemByIdQuery"                           → no match (no Command suffix)
    /// </summary>
    private static bool TryGetLookupInterfaceName(string commandName, out string lookupName)
    {
        lookupName = string.Empty;

        if (string.IsNullOrEmpty(commandName) || !commandName.EndsWith(CommandSuffix, StringComparison.Ordinal))
            return false;

        string verbPrefix;
        if (commandName.StartsWith(CreatePrefix, StringComparison.Ordinal))
            verbPrefix = CreatePrefix;
        else if (commandName.StartsWith(UpdatePrefix, StringComparison.Ordinal))
            verbPrefix = UpdatePrefix;
        else if (commandName.StartsWith(DeletePrefix, StringComparison.Ordinal))
            verbPrefix = DeletePrefix;
        else
            return false;

        var entityLength = commandName.Length - verbPrefix.Length - CommandSuffix.Length;
        if (entityLength <= 0)
            return false;

        var entityName = commandName.Substring(verbPrefix.Length, entityLength);
        lookupName = "I" + entityName + "Lookup";
        return true;
    }
}
