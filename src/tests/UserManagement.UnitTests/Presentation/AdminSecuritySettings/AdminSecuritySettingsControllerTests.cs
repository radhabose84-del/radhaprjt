using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserManagement.Application.AdminSecuritySettings.Commands.CreateAdminSecuritySettings;
using UserManagement.Application.AdminSecuritySettings.Commands.DeleteAdminSecuritySettings;
using UserManagement.Application.AdminSecuritySettings.Commands.UpdateAdminSecuritySettings;
using UserManagement.Application.AdminSecuritySettings.Queries.GetAdminSecuritySettings;
using UserManagement.Application.AdminSecuritySettings.Queries.GetAdminSecuritySettingsById;
using UserManagement.Presentation.Controllers;

namespace UserManagement.UnitTests.Presentation.AdminSecuritySettings
{
    public sealed class AdminSecuritySettingsControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<AdminSecuritySettingsController>> _mockLogger = new();

        private AdminSecuritySettingsController CreateSut() =>
            new(_mockMediator.Object, _mockLogger.Object);

        [Fact]
        public async Task GetAll_WithData_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAdminSecuritySettingsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetAdminSecuritySettingsDto>>
                {
                    IsSuccess = true,
                    Data = new List<GetAdminSecuritySettingsDto> { new() },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllAdminSecuritySettingsAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_EmptyData_ReturnsNotFound()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAdminSecuritySettingsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetAdminSecuritySettingsDto>>
                {
                    IsSuccess = false,
                    Data = new List<GetAdminSecuritySettingsDto>()
                });

            var result = await CreateSut().GetAllAdminSecuritySettingsAsync(1, 10);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAdminSecuritySettingsByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetAdminSecuritySettingsDto());

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateAdminSecuritySettingsCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateAsync(new CreateAdminSecuritySettingsCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateAdminSecuritySettingsCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().UpdateAsync(1, new UpdateAdminSecuritySettingsCommand { Id = 1 });

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAdminSecuritySettingsByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetAdminSecuritySettingsDto());

            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteAdminSecuritySettingsCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().Delete(1);

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
