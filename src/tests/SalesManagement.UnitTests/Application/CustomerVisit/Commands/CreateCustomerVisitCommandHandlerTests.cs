using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SalesManagement.Application.Common.Interfaces.ICustomerVisit;
using SalesManagement.Application.CustomerVisit.Commands.CreateCustomerVisit;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.CustomerVisit.Commands
{
    public class CreateCustomerVisitCommandHandlerTests
    {
        private readonly Mock<ICustomerVisitCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<ICustomerVisitQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<ILogger<CreateCustomerVisitCommandHandler>> _mockLogger = new();

        private CreateCustomerVisitCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object, _mockLogger.Object);

        private static CreateCustomerVisitCommand ValidCommand() => new()
        {
            CustomerId = 1,
            VisitTypeId = 2,
            VisitDateTime = DateTimeOffset.UtcNow,
            Latitude = 12.97m,
            Longitude = 77.59m,
            Remarks = "Test visit",
            MarketingOfficerId = 3
        };

        private void SetupMapper(CreateCustomerVisitCommand cmd)
        {
            _mockMapper
                .Setup(m => m.Map<SalesManagement.Domain.Entities.CustomerVisit>(cmd))
                .Returns(new SalesManagement.Domain.Entities.CustomerVisit
                {
                    CustomerId = cmd.CustomerId,
                    VisitTypeId = cmd.VisitTypeId,
                    MarketingOfficerId = cmd.MarketingOfficerId
                });
        }

        private void SetupCreateAsync(int returnId = 1)
        {
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.CustomerVisit>()))
                .ReturnsAsync(returnId);
        }

        private void SetupPublishAudit()
        {
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_NoImage_ReturnsSuccess()
        {
            var command = ValidCommand();
            SetupMapper(command);
            SetupCreateAsync(1);
            SetupPublishAudit();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_NoImage_ReturnsNewEntityId()
        {
            var command = ValidCommand();
            SetupMapper(command);
            SetupCreateAsync(42);
            SetupPublishAudit();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Data.Should().Be(42);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateAsync_Once()
        {
            var command = ValidCommand();
            SetupMapper(command);
            SetupCreateAsync(1);
            SetupPublishAudit();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.CustomerVisit>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditLogEvent_Once()
        {
            var command = ValidCommand();
            SetupMapper(command);
            SetupCreateAsync(1);
            SetupPublishAudit();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "CUSTOMER_VISIT_CREATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsCorrectMessage()
        {
            var command = ValidCommand();
            SetupMapper(command);
            SetupCreateAsync(1);
            SetupPublishAudit();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Message.Should().Contain("created successfully");
        }
    }
}
