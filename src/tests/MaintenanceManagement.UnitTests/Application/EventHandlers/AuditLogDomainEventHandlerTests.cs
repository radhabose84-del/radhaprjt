using Contracts.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces;
using MaintenanceManagement.Application.EventHandlers;
using MaintenanceManagement.Domain.Events;
using MongoDB.Driver;

namespace MaintenanceManagement.UnitTests.Application.EventHandlers
{
    // AuditLogDomainEventHandler (class name: DomainEventHandler) persists audit entries to MongoDB.
    // It depends on IMongoDbContext (IMongoCollection<dynamic>.InsertOneAsync), IIPAddressService, ITimeZoneService.
    //
    // NOTE: IMongoCollection<dynamic>.InsertOneAsync cannot be reliably mocked via expression trees
    // because "dynamic" is not allowed in Linq Expression<Func<>> arguments. These tests are therefore
    // limited to construction-time smoke tests and interface compliance verification.
    public sealed class AuditLogDomainEventHandlerTests
    {
        private readonly Mock<IMongoDbContext> _mockMongoContext = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService> _mockTz = new(MockBehavior.Loose);

        private DomainEventHandler CreateSut() =>
            new(_mockMongoContext.Object, _mockIp.Object, _mockTz.Object);

        [Fact]
        public void Handler_CanBeInstantiated()
        {
            var sut = CreateSut();
            sut.Should().NotBeNull();
        }

        [Fact]
        public void Handler_ImplementsINotificationHandler()
        {
            typeof(MediatR.INotificationHandler<AuditLogsDomainEvent>)
                .IsAssignableFrom(typeof(DomainEventHandler))
                .Should().BeTrue();
        }

        [Fact]
        public void Handler_HasExpectedConstructorDependencies()
        {
            var ctors = typeof(DomainEventHandler).GetConstructors();
            ctors.Should().HaveCount(1);

            var parameters = ctors[0].GetParameters();
            parameters.Should().HaveCount(3);
            parameters.Should().Contain(p => p.ParameterType == typeof(IMongoDbContext));
            parameters.Should().Contain(p => p.ParameterType == typeof(IIPAddressService));
            parameters.Should().Contain(p => p.ParameterType == typeof(ITimeZoneService));
        }

        [Fact]
        public void Handle_Method_AcceptsAuditLogsDomainEvent()
        {
            var handleMethod = typeof(DomainEventHandler).GetMethod(nameof(DomainEventHandler.Handle));
            handleMethod.Should().NotBeNull();

            var parameters = handleMethod!.GetParameters();
            parameters.Should().HaveCount(2);
            parameters[0].ParameterType.Should().Be(typeof(AuditLogsDomainEvent));
            parameters[1].ParameterType.Should().Be(typeof(CancellationToken));
        }
    }
}
