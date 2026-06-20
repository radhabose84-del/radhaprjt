using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IGlAccountMaster;
using FinanceManagement.Application.GlAccountMaster.Commands.RemoveGlAccountFavourite;

namespace FinanceManagement.UnitTests.Application.GlAccountMaster.Commands
{
    public sealed class RemoveGlAccountFavouriteCommandHandlerTests
    {
        private readonly Mock<IGlAccountUserPrefStore> _mockStore = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        [Fact]
        public async Task Handle_RemovesFavourite_ForTokenUserAndCompany()
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            _mockIp.Setup(s => s.GetUserId()).Returns(396);
            _mockStore.Setup(s => s.RemoveFavouriteAsync(396, 1, 42, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var sut = new RemoveGlAccountFavouriteCommandHandler(_mockStore.Object, _mockIp.Object, _mockMediator.Object);
            var result = await sut.Handle(new RemoveGlAccountFavouriteCommand { GlAccountMasterId = 42 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            _mockStore.Verify(s => s.RemoveFavouriteAsync(396, 1, 42, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
