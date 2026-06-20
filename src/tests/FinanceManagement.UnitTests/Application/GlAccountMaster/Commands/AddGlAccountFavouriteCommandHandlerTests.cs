using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IGlAccountMaster;
using FinanceManagement.Application.GlAccountMaster.Commands.AddGlAccountFavourite;

namespace FinanceManagement.UnitTests.Application.GlAccountMaster.Commands
{
    public sealed class AddGlAccountFavouriteCommandHandlerTests
    {
        private readonly Mock<IGlAccountUserPrefStore> _mockStore = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private AddGlAccountFavouriteCommandHandler CreateSut() =>
            new(_mockStore.Object, _mockIp.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_AddsFavourite_ForTokenUserAndCompany_AndAudits()
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            _mockIp.Setup(s => s.GetUserId()).Returns(396);
            _mockStore.Setup(s => s.AddFavouriteAsync(396, 1, 42, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new AddGlAccountFavouriteCommand { GlAccountMasterId = 42 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            _mockStore.Verify(s => s.AddFavouriteAsync(396, 1, 42, It.IsAny<CancellationToken>()), Times.Once);
            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
