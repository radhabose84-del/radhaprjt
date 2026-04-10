using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.CompanySettings.Commands.CreateCompanySettings;
using UserManagement.Application.CompanySettings.Commands.UpdateCompanySettings;
using UserManagement.Application.CompanySettings.Queries.GetCompanySettings;
using UserManagement.Application.CompanySettings.Queries.GetCompanySettingsById;
using UserManagement.Presentation.Controllers;

namespace UserManagement.UnitTests.Controllers
{
    public sealed class CompanySettingsControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);

        private CompanySettingsController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task CreateAsync_ReturnsOkResult()
        {
            // Arrange
            var command = new CreateCompanySettingsCommand();
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateCompanySettingsCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var sut = CreateSut();

            // Act
            var result = await sut.CreateAsync(command);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_CallsMediatorSend_Once()
        {
            // Arrange
            var command = new CreateCompanySettingsCommand();
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateCompanySettingsCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var sut = CreateSut();

            // Act
            await sut.CreateAsync(command);

            // Assert
            _mockMediator.Verify(
                m => m.Send(It.IsAny<CreateCompanySettingsCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            // Arrange
            var command = new UpdateCompanySettingsCommand();
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateCompanySettingsCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var sut = CreateSut();

            // Act
            var result = await sut.Update(command);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetByIdAsync_WithData_ReturnsOkResult()
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetCompanySettingByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<CompanySettingsDTO>
                {
                    IsSuccess = true,
                    Data = new CompanySettingsDTO()
                });

            var sut = CreateSut();

            // Act
            var result = await sut.GetByIdAsync();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetByIdAsync_NullData_ReturnsNotFound()
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetCompanySettingByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ApiResponseDTO<CompanySettingsDTO>?)null);

            var sut = CreateSut();

            // Act
            var result = await sut.GetByIdAsync();

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }
    }
}
