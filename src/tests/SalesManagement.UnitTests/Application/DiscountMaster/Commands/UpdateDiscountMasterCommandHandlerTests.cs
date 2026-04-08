using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDiscountMaster;
using SalesManagement.Application.DiscountMaster.Commands.CreateDiscountMaster;
using SalesManagement.Application.DiscountMaster.Commands.UpdateDiscountMaster;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.DiscountMaster.Commands
{
    public class UpdateDiscountMasterCommandHandlerTests
    {
        private readonly Mock<IDiscountMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IDiscountMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        private UpdateDiscountMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupMapper(UpdateDiscountMasterCommand cmd)
        {
            _mockMapper
                .Setup(m => m.Map<SalesManagement.Domain.Entities.DiscountMaster>(cmd))
                .Returns(new SalesManagement.Domain.Entities.DiscountMaster
                {
                    Id = cmd.Id,
                    DiscountName = cmd.DiscountName,
                    TriggerEventId = cmd.TriggerEventId,
                    DiscountBasisId = cmd.DiscountBasisId,
                    ExecutionTypeId = cmd.ExecutionTypeId,
                    ValueTypeId = cmd.ValueTypeId,
                    SlabTypeId = cmd.SlabTypeId,
                    Priority = cmd.Priority
                });
        }

        private void SetupUpdateAsync(int returnId = 1)
        {
            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<SalesManagement.Domain.Entities.DiscountMaster>()))
                .ReturnsAsync(returnId);
        }

        private void SetupPublishAudit()
        {
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        private static UpdateDiscountMasterCommand ValidCommand() => new()
        {
            Id = 1,
            DiscountName = "Updated Discount",
            TriggerEventId = 1,
            DiscountBasisId = 2,
            ExecutionTypeId = 3,
            ValueTypeId = 4,
            SlabTypeId = 5,
            Priority = 1,
            IsActive = 1,
            Slabs = new List<DiscountSlabItem>
            {
                new() { SlabOrder = 1, FromValue = 0, ToValue = 100, DiscountValue = 10 }
            }
        };

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            var command = ValidCommand();
            SetupMapper(command);
            SetupUpdateAsync(1);
            SetupPublishAudit();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsUpdatedId()
        {
            var command = ValidCommand();
            SetupMapper(command);
            SetupUpdateAsync(1);
            SetupPublishAudit();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Data.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateAsync_Once()
        {
            var command = ValidCommand();
            SetupMapper(command);
            SetupUpdateAsync(1);
            SetupPublishAudit();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<SalesManagement.Domain.Entities.DiscountMaster>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditLogEvent_Once()
        {
            var command = ValidCommand();
            SetupMapper(command);
            SetupUpdateAsync(1);
            SetupPublishAudit();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "DISCOUNT_MASTER_UPDATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsCorrectMessage()
        {
            var command = ValidCommand();
            SetupMapper(command);
            SetupUpdateAsync(1);
            SetupPublishAudit();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Message.Should().Contain("updated successfully");
        }
    }
}
