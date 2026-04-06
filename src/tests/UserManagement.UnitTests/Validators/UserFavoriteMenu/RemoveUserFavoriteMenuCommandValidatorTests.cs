using Contracts.Interfaces;
using FluentValidation.TestHelper;
using UserManagement.Application.Common.Interfaces.IUserFavoriteMenu;
using UserManagement.Application.UserFavoriteMenu.Commands.RemoveUserFavoriteMenu;
using UserManagement.Presentation.Validation.UserFavoriteMenu;

namespace UserManagement.UnitTests.Validators.UserFavoriteMenu
{
    public sealed class RemoveUserFavoriteMenuCommandValidatorTests
    {
        private readonly Mock<IUserFavoriteMenuQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);

        private RemoveUserFavoriteMenuCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object, _mockIpService.Object);

        private void SetupHappyPath(int menuId = 1, int userId = 1)
        {
            _mockIpService.Setup(s => s.GetUserId()).Returns(userId);
            _mockQueryRepo.Setup(r => r.FavoriteExistsAsync(userId, menuId)).ReturnsAsync(true);
        }

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            SetupHappyPath();
            var result = await CreateValidator().TestValidateAsync(new RemoveUserFavoriteMenuCommand(1));
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task ZeroMenuId_FailsValidation()
        {
            var result = await CreateValidator().TestValidateAsync(new RemoveUserFavoriteMenuCommand(0));
            result.ShouldHaveValidationErrorFor(x => x.MenuId);
        }

        [Fact]
        public async Task FavoriteNotExists_FailsValidation()
        {
            _mockIpService.Setup(s => s.GetUserId()).Returns(1);
            _mockQueryRepo.Setup(r => r.FavoriteExistsAsync(1, 1)).ReturnsAsync(false);
            var result = await CreateValidator().TestValidateAsync(new RemoveUserFavoriteMenuCommand(1));
            result.ShouldHaveAnyValidationError();
        }
    }
}
