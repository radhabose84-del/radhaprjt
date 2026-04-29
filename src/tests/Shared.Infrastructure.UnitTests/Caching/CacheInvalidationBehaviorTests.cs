using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Shared.Infrastructure.Caching;

namespace Shared.Infrastructure.UnitTests.Caching;

public sealed class CacheInvalidationBehaviorTests
{
    // Test command/query types — names matter, the behavior parses them by convention.
    public sealed record CreateItemCommand : IRequest<int>;
    public sealed record UpdateCustomerCommand : IRequest<int>;
    public sealed record DeleteAgentCommand : IRequest<bool>;
    public sealed record CreateAgentCommissionConfigurationCommand : IRequest<int>;
    public sealed record GetItemByIdQuery : IRequest<int>;
    public sealed record ApproveOrderCommand : IRequest<int>;
    public sealed record CreateUnknownEntityCommand : IRequest<int>;
    public sealed record CreateWorkflowCommand : IRequest<int>;       // matches excluded IWorkflowLookup
    public sealed record CreateDocumentSequenceCommand : IRequest<int>; // matches excluded IDocumentSequenceLookup

    private static readonly object _staticStateLock = new();

    /// <summary>
    /// Captures and restores the global static state (RegisteredCachedLookupNames /
    /// ExcludedLookupInterfaces) so tests can run in parallel without corrupting each other.
    /// </summary>
    private static void RunWithRegistered(Action body, params string[] cachedLookupNames)
    {
        lock (_staticStateLock)
        {
            var snapshotRegistered = new HashSet<string>(
                CachingServiceExtensions.RegisteredCachedLookupNames,
                StringComparer.Ordinal);

            try
            {
                CachingServiceExtensions.RegisteredCachedLookupNames.Clear();
                foreach (var n in cachedLookupNames)
                    CachingServiceExtensions.RegisteredCachedLookupNames.Add(n);

                body();
            }
            finally
            {
                CachingServiceExtensions.RegisteredCachedLookupNames.Clear();
                foreach (var n in snapshotRegistered)
                    CachingServiceExtensions.RegisteredCachedLookupNames.Add(n);
            }
        }
    }

    private static (CacheInvalidationBehavior<TRequest, TResponse> sut, Mock<ILogger<CacheInvalidationBehavior<TRequest, TResponse>>> logger)
        CreateSut<TRequest, TResponse>(LookupCacheInvalidator? invalidator = null)
        where TRequest : notnull
    {
        invalidator ??= new LookupCacheInvalidator(NullLogger<LookupCacheInvalidator>.Instance);
        var logger = new Mock<ILogger<CacheInvalidationBehavior<TRequest, TResponse>>>();
        var sut = new CacheInvalidationBehavior<TRequest, TResponse>(invalidator, logger.Object);
        return (sut, logger);
    }

    private static RequestHandlerDelegate<TResponse> SuccessfulHandler<TResponse>(TResponse value) =>
        () => Task.FromResult(value);

    private static RequestHandlerDelegate<TResponse> FailingHandler<TResponse>(Exception ex) =>
        () => Task.FromException<TResponse>(ex);

    [Fact]
    public async Task Handle_CreateItemCommand_EvictsIItemLookup()
    {
        await Task.Run(() => RunWithRegistered(() =>
        {
            var invalidator = new LookupCacheInvalidator(NullLogger<LookupCacheInvalidator>.Instance);
            var oldToken = invalidator.GetEvictionToken("IItemLookup");
            var (sut, _) = CreateSut<CreateItemCommand, int>(invalidator);

            sut.Handle(new CreateItemCommand(), SuccessfulHandler(42), CancellationToken.None)
                .GetAwaiter().GetResult();

            // After eviction, the old token is cancelled and a new one is issued
            oldToken.IsCancellationRequested.Should().BeTrue();
            var newToken = invalidator.GetEvictionToken("IItemLookup");
            newToken.IsCancellationRequested.Should().BeFalse();
        }, "IItemLookup"));
    }

    [Fact]
    public async Task Handle_UpdateCustomerCommand_EvictsICustomerLookup()
    {
        await Task.Run(() => RunWithRegistered(() =>
        {
            var invalidator = new LookupCacheInvalidator(NullLogger<LookupCacheInvalidator>.Instance);
            var oldToken = invalidator.GetEvictionToken("ICustomerLookup");
            var (sut, _) = CreateSut<UpdateCustomerCommand, int>(invalidator);

            sut.Handle(new UpdateCustomerCommand(), SuccessfulHandler(1), CancellationToken.None)
                .GetAwaiter().GetResult();

            oldToken.IsCancellationRequested.Should().BeTrue();
        }, "ICustomerLookup"));
    }

    [Fact]
    public async Task Handle_DeleteAgentCommand_EvictsIAgentLookup()
    {
        await Task.Run(() => RunWithRegistered(() =>
        {
            var invalidator = new LookupCacheInvalidator(NullLogger<LookupCacheInvalidator>.Instance);
            var oldToken = invalidator.GetEvictionToken("IAgentLookup");
            var (sut, _) = CreateSut<DeleteAgentCommand, bool>(invalidator);

            sut.Handle(new DeleteAgentCommand(), SuccessfulHandler(true), CancellationToken.None)
                .GetAwaiter().GetResult();

            oldToken.IsCancellationRequested.Should().BeTrue();
        }, "IAgentLookup"));
    }

    [Fact]
    public async Task Handle_LongCompoundEntityName_MapsCorrectly()
    {
        await Task.Run(() => RunWithRegistered(() =>
        {
            var invalidator = new LookupCacheInvalidator(NullLogger<LookupCacheInvalidator>.Instance);
            var oldToken = invalidator.GetEvictionToken("IAgentCommissionConfigurationLookup");
            var (sut, _) = CreateSut<CreateAgentCommissionConfigurationCommand, int>(invalidator);

            sut.Handle(new CreateAgentCommissionConfigurationCommand(), SuccessfulHandler(1), CancellationToken.None)
                .GetAwaiter().GetResult();

            oldToken.IsCancellationRequested.Should().BeTrue();
        }, "IAgentCommissionConfigurationLookup"));
    }

    [Fact]
    public async Task Handle_GetItemByIdQuery_DoesNotEvict()
    {
        await Task.Run(() => RunWithRegistered(() =>
        {
            var invalidator = new LookupCacheInvalidator(NullLogger<LookupCacheInvalidator>.Instance);
            var token = invalidator.GetEvictionToken("IItemLookup");
            var (sut, _) = CreateSut<GetItemByIdQuery, int>(invalidator);

            sut.Handle(new GetItemByIdQuery(), SuccessfulHandler(99), CancellationToken.None)
                .GetAwaiter().GetResult();

            token.IsCancellationRequested.Should().BeFalse();
        }, "IItemLookup"));
    }

    [Fact]
    public async Task Handle_ApproveOrderCommand_DoesNotEvict()
    {
        await Task.Run(() => RunWithRegistered(() =>
        {
            var invalidator = new LookupCacheInvalidator(NullLogger<LookupCacheInvalidator>.Instance);
            var token = invalidator.GetEvictionToken("IOrderLookup");
            var (sut, _) = CreateSut<ApproveOrderCommand, int>(invalidator);

            sut.Handle(new ApproveOrderCommand(), SuccessfulHandler(1), CancellationToken.None)
                .GetAwaiter().GetResult();

            token.IsCancellationRequested.Should().BeFalse();
        }, "IOrderLookup"));
    }

    [Fact]
    public async Task Handle_UnregisteredLookup_DoesNotEvict()
    {
        await Task.Run(() => RunWithRegistered(() =>
        {
            var invalidator = new LookupCacheInvalidator(NullLogger<LookupCacheInvalidator>.Instance);
            var token = invalidator.GetEvictionToken("IUnknownEntityLookup");
            var (sut, _) = CreateSut<CreateUnknownEntityCommand, int>(invalidator);

            sut.Handle(new CreateUnknownEntityCommand(), SuccessfulHandler(1), CancellationToken.None)
                .GetAwaiter().GetResult();

            // Lookup name not in RegisteredCachedLookupNames → silent skip → token stays alive
            token.IsCancellationRequested.Should().BeFalse();
        }
        // intentionally NOT registering "IUnknownEntityLookup"
        ));
    }

    [Fact]
    public async Task Handle_ExcludedLookup_Workflow_DoesNotEvict()
    {
        // Even if we (hypothetically) had IWorkflowLookup in RegisteredCachedLookupNames,
        // the exclusion list takes precedence. In practice IWorkflowLookup is never registered,
        // so this test simulates the worst case to prove the exclusion guard works.
        await Task.Run(() => RunWithRegistered(() =>
        {
            CachingServiceExtensions.ExcludedLookupInterfaces.Should().Contain("IWorkflowLookup");

            var invalidator = new LookupCacheInvalidator(NullLogger<LookupCacheInvalidator>.Instance);
            var token = invalidator.GetEvictionToken("IWorkflowLookup");
            var (sut, _) = CreateSut<CreateWorkflowCommand, int>(invalidator);

            sut.Handle(new CreateWorkflowCommand(), SuccessfulHandler(1), CancellationToken.None)
                .GetAwaiter().GetResult();

            token.IsCancellationRequested.Should().BeFalse();
        }, "IWorkflowLookup"));
    }

    [Fact]
    public async Task Handle_ExcludedLookup_DocumentSequence_DoesNotEvict()
    {
        await Task.Run(() => RunWithRegistered(() =>
        {
            CachingServiceExtensions.ExcludedLookupInterfaces.Should().Contain("IDocumentSequenceLookup");

            var invalidator = new LookupCacheInvalidator(NullLogger<LookupCacheInvalidator>.Instance);
            var token = invalidator.GetEvictionToken("IDocumentSequenceLookup");
            var (sut, _) = CreateSut<CreateDocumentSequenceCommand, int>(invalidator);

            sut.Handle(new CreateDocumentSequenceCommand(), SuccessfulHandler(1), CancellationToken.None)
                .GetAwaiter().GetResult();

            token.IsCancellationRequested.Should().BeFalse();
        }, "IDocumentSequenceLookup"));
    }

    [Fact]
    public async Task Handle_HandlerThrows_DoesNotEvict()
    {
        await Task.Run(() => RunWithRegistered(() =>
        {
            var invalidator = new LookupCacheInvalidator(NullLogger<LookupCacheInvalidator>.Instance);
            var token = invalidator.GetEvictionToken("IItemLookup");
            var (sut, _) = CreateSut<CreateItemCommand, int>(invalidator);

            var thrown = new InvalidOperationException("simulated handler failure");

            Func<Task> act = async () => await sut.Handle(
                new CreateItemCommand(),
                FailingHandler<int>(thrown),
                CancellationToken.None);

            act.Should().ThrowAsync<InvalidOperationException>().GetAwaiter().GetResult();

            // Eviction must NOT happen — DB transaction was rolled back, cache must stay
            token.IsCancellationRequested.Should().BeFalse();
        }, "IItemLookup"));
    }

    [Fact]
    public async Task Handle_HandlerSucceedsButInvalidatorThrows_ResponseIsNotImpacted()
    {
        await Task.Run(() => RunWithRegistered(() =>
        {
            var faultyInvalidator = new Mock<LookupCacheInvalidator>(NullLogger<LookupCacheInvalidator>.Instance)
            { CallBase = false };
            faultyInvalidator
                .Setup(i => i.Evict(It.IsAny<string>()))
                .Throws(new InvalidOperationException("simulated cache failure"));

            var (sut, logger) = CreateSut<CreateItemCommand, int>(faultyInvalidator.Object);

            var result = sut.Handle(new CreateItemCommand(), SuccessfulHandler(7), CancellationToken.None)
                .GetAwaiter().GetResult();

            // The successful response must be preserved — eviction failure must NEVER bubble up
            result.Should().Be(7);
        }, "IItemLookup"));
    }

    [Fact]
    public async Task Handle_NotACommand_DoesNotEvict()
    {
        // GetItemByIdQuery doesn't end with "Command" — silent skip
        await Task.Run(() => RunWithRegistered(() =>
        {
            var invalidator = new LookupCacheInvalidator(NullLogger<LookupCacheInvalidator>.Instance);
            var token = invalidator.GetEvictionToken("IItemLookup");
            var (sut, _) = CreateSut<GetItemByIdQuery, int>(invalidator);

            sut.Handle(new GetItemByIdQuery(), SuccessfulHandler(1), CancellationToken.None)
                .GetAwaiter().GetResult();

            token.IsCancellationRequested.Should().BeFalse();
        }, "IItemLookup"));
    }
}
