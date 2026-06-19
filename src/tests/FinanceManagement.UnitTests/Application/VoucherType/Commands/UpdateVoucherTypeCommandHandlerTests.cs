using FinanceManagement.Application.Common.Interfaces.IVoucherTypeMaster;
using FinanceManagement.Application.VoucherType.Commands.UpdateVoucherType;
using FinanceManagement.UnitTests.TestData;

namespace FinanceManagement.UnitTests.Application.VoucherType.Commands
{
    public sealed class UpdateVoucherTypeCommandHandlerTests
    {
        private readonly Mock<IVoucherTypeMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateVoucherTypeCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int updatedId = 1)
        {
            _mockMapper
                .Setup(m => m.Map<FinanceManagement.Domain.Entities.VoucherTypeMaster>(It.IsAny<UpdateVoucherTypeCommand>()))
                .Returns(VoucherTypeBuilders.ValidEntity());
            _mockCommandRepo
                .Setup(r => r.UpdateAsync(
                    It.IsAny<FinanceManagement.Domain.Entities.VoucherTypeMaster>(),
                    It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(updatedId);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(VoucherTypeBuilders.ValidUpdateCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Contain("updated successfully");
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(VoucherTypeBuilders.ValidUpdateCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(
                    It.IsAny<FinanceManagement.Domain.Entities.VoucherTypeMaster>(),
                    It.IsAny<IEnumerable<int>>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(VoucherTypeBuilders.ValidUpdateCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Update" &&
                        e.ActionCode == "VOUCHER_TYPE_UPDATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
