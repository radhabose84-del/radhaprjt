using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDiscountMaster;
using SalesManagement.Application.DiscountMaster.Commands.CreateDiscountMaster;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.DiscountMaster.Commands
{
    public class CreateDiscountMasterCommandHandlerTests
    {
        private readonly Mock<IDiscountMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IDiscountMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        private CreateDiscountMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupMapper(CreateDiscountMasterCommand cmd)
        {
            _mockMapper
                .Setup(m => m.Map<SalesManagement.Domain.Entities.DiscountMaster>(cmd))
                .Returns(new SalesManagement.Domain.Entities.DiscountMaster
                {
                    DiscountName = cmd.DiscountName,
                    TriggerEventId = cmd.TriggerEventId,
                    DiscountBasisId = cmd.DiscountBasisId,
                    ExecutionTypeId = cmd.ExecutionTypeId,
                    ValueTypeId = cmd.ValueTypeId,
                    SlabTypeId = cmd.SlabTypeId,
                    Priority = cmd.Priority
                });
        }

        private void SetupCreateAsync(int returnId = 1)
        {
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.DiscountMaster>()))
                .ReturnsAsync(returnId);
        }

        private void SetupPublishAudit()
        {
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        private static CreateDiscountMasterCommand ValidCommand() => new()
        {
            DiscountName = "Test Discount",
            TriggerEventId = 1,
            DiscountBasisId = 2,
            ExecutionTypeId = 3,
            ValueTypeId = 4,
            SlabTypeId = 5,
            Priority = 1,
            Slabs = new List<DiscountSlabItem>
            {
                new() { SlabOrder = 1, FromValue = 0, ToValue = 100, DiscountValue = 5 }
            }
        };

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
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
        public async Task Handle_ValidCommand_ReturnsNewEntityId()
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
                r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.DiscountMaster>()),
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
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "DISCOUNT_MASTER_CREATE"),
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
