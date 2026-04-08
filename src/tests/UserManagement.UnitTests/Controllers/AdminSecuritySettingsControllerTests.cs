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

namespace UserManagement.UnitTests.Controllers
{
    public sealed class AdminSecuritySettingsControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<AdminSecuritySettingsController>> _mockLogger = new();

        private AdminSecuritySettingsController CreateSut() =>
            new(_mockMediator.Object, _mockLogger.Object);

        [Fact]
        public async Task GetAllAdminSecuritySettingsAsync_ReturnsOkResult()
        {
            // Arrange
            var dto = new GetAdminSecuritySettingsDto();
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAdminSecuritySettingsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetAdminSecuritySettingsDto>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<GetAdminSecuritySettingsDto> { dto },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            var sut = CreateSut();

            // Act
            var result = await sut.GetAllAdminSecuritySettingsAsync(1, 10);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllAdminSecuritySettingsAsync_EmptyData_ReturnsNotFound()
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAdminSecuritySettingsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetAdminSecuritySettingsDto>>
                {
                    IsSuccess = true,
                    Message = "No records found",
                    Data = new List<GetAdminSecuritySettingsDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var sut = CreateSut();

            // Act
            var result = await sut.GetAllAdminSecuritySettingsAsync(1, 10);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsOkResult()
        {
            // Arrange
            var dto = new GetAdminSecuritySettingsDto();
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAdminSecuritySettingsByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            var sut = CreateSut();

            // Act
            var result = await sut.GetByIdAsync(1);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_ReturnsOkResult()
        {
            // Arrange
            var command = new CreateAdminSecuritySettingsCommand();
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateAdminSecuritySettingsCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var sut = CreateSut();

            // Act
            var result = await sut.CreateAsync(command);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateAsync_ReturnsOkResult()
        {
            // Arrange
            var command = new UpdateAdminSecuritySettingsCommand { Id = 1 };
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateAdminSecuritySettingsCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var sut = CreateSut();

            // Act
            var result = await sut.UpdateAsync(1, command);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateAsync_IdMismatch_ReturnsBadRequest()
        {
            // Arrange
            var command = new UpdateAdminSecuritySettingsCommand { Id = 2 };
            var sut = CreateSut();

            // Act
            var result = await sut.UpdateAsync(1, command);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Delete_WithExistingRecord_ReturnsOkResult()
        {
            // Arrange
            var dto = new GetAdminSecuritySettingsDto();
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAdminSecuritySettingsByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteAdminSecuritySettingsCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var sut = CreateSut();

            // Act
            var result = await sut.Delete(1);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_WithNonExistingRecord_ReturnsNotFound()
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAdminSecuritySettingsByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetAdminSecuritySettingsDto?)null);

            var sut = CreateSut();

            // Act
            var result = await sut.Delete(999);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }
    }
}
