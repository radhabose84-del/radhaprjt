using Contracts.Interfaces;
using FluentValidation.TestHelper;
using UserManagement.Application.Common.Interfaces.IUserFavoriteMenu;
using UserManagement.Application.UserFavoriteMenu.Commands.AddUserFavoriteMenu;
using UserManagement.Presentation.Validation.UserFavoriteMenu;

namespace UserManagement.UnitTests.Validators.UserFavoriteMenu
{
    public sealed class AddUserFavoriteMenuCommandValidatorTests
    {
        private readonly Mock<IUserFavoriteMenuQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);

        private AddUserFavoriteMenuCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object, _mockIpService.Object);

        private static AddUserFavoriteMenuCommand ValidCommand() =>
            new AddUserFavoriteMenuCommand { MenuId = 1 };

        private void SetupHappyPath(int menuId = 1, int userId = 1)
        {
            _mockIpService.Setup(s => s.GetUserId()).Returns(userId);
            _mockQueryRepo.Setup(r => r.MenuExistsAndActiveAsync(menuId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.AlreadyFavoritedAsync(userId, menuId)).ReturnsAsync(false);
        }

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            SetupHappyPath();
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task ZeroMenuId_FailsValidation()
        {
            var result = await CreateValidator().TestValidateAsync(new AddUserFavoriteMenuCommand { MenuId = 0 });
            result.ShouldHaveValidationErrorFor(x => x.MenuId);
        }

        [Fact]
        public async Task MenuNotFound_FailsValidation()
        {
            _mockIpService.Setup(s => s.GetUserId()).Returns(1);
            _mockQueryRepo.Setup(r => r.MenuExistsAndActiveAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.AlreadyFavoritedAsync(1, 1)).ReturnsAsync(false);
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task AlreadyFavorited_FailsValidation()
        {
            _mockIpService.Setup(s => s.GetUserId()).Returns(1);
            _mockQueryRepo.Setup(r => r.MenuExistsAndActiveAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.AlreadyFavoritedAsync(1, 1)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldHaveAnyValidationError();
        }
    }
}
