using ProductionManagement.Application.Common.Interfaces.IRepackingHeader;
using ProductionManagement.Application.RepackingHeader.Commands.CreateRepackingHeader;
using ProductionManagement.Application.RepackingHeader.Commands.UpdateRepackingHeader;

namespace ProductionManagement.UnitTests.Application.Repacking.Commands
{
    public sealed class UpdateRepackingHeaderCommandHandlerTests
    {
        private readonly Mock<IRepackingHeaderCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IRepackingHeaderQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateRepackingHeaderCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private static UpdateRepackingHeaderCommand BuildValidCommand(int id = 1) => new()
        {
            Id = id,
            RepackDate = DateOnly.FromDateTime(DateTime.Today),
            ItemId = 1,
            OldItemId = 1,
            OldPackTypeId = 1,
            PackTypeId = 2,
            NetWeightPerPack = 25m,
            TotalBags = 20,
            NetWeight = 500m,
            WarehouseId = 2,
            BinId = 2,
            LooseConeKgs = 0m,
            Remarks = "Updated repack",
            IsActive = 1,
            Details = new List<CreateRepackingDetailItem>
            {
                new() { OldStartPackNo = 1, OldEndPackNo = 10 }
            }
        };

        private void SetupHappyPath(int returnId = 1)
        {
            _mockMapper.Setup(m => m.Map<ProductionManagement.Domain.Entities.RepackingHeader>(It.IsAny<UpdateRepackingHeaderCommand>()))
                .Returns(new ProductionManagement.Domain.Entities.RepackingHeader { Id = 1 });

            _mockQueryRepo.Setup(r => r.IsRepackingHeaderLinkedAsync(It.IsAny<int>()))
                .ReturnsAsync(false);

            _mockCommandRepo.Setup(r => r.UpdateAsync(It.IsAny<ProductionManagement.Domain.Entities.RepackingHeader>()))
                .ReturnsAsync(returnId);

            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(BuildValidCommand(), CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Be("Repacking updated successfully.");
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(BuildValidCommand(), CancellationToken.None);
            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<ProductionManagement.Domain.Entities.RepackingHeader>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(BuildValidCommand(), CancellationToken.None);
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Update" &&
                        e.ActionCode == "REPACKING_UPDATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_YarnConversion_PublishesCorrectAuditCode()
        {
            SetupHappyPath();
            var command = BuildValidCommand();
            command.ItemId = 2;
            command.OldItemId = 1;

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Message.Should().Be("Yarn Conversion updated successfully.");
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "YARN_CONVERSION_UPDATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_InactivateWhenLinked_ThrowsException()
        {
            _mockQueryRepo.Setup(r => r.IsRepackingHeaderLinkedAsync(1)).ReturnsAsync(true);

            var sut = CreateSut();
            var command = BuildValidCommand();
            command.IsActive = 0;

            Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*cannot inactivate*");
        }

        [Fact]
        public async Task Handle_InactivateWhenNotLinked_Succeeds()
        {
            SetupHappyPath();
            var command = BuildValidCommand();
            command.IsActive = 0;

            var result = await CreateSut().Handle(command, CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_MapsCommandToEntity()
        {
            SetupHappyPath();
            var command = BuildValidCommand(5);
            await CreateSut().Handle(command, CancellationToken.None);
            _mockMapper.Verify(m => m.Map<ProductionManagement.Domain.Entities.RepackingHeader>(command), Times.Once);
        }
    }
}
