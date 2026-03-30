using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.CompanySettings.Commands.CreateCompanySettings;
using UserManagement.Application.CompanySettings.Commands.UpdateCompanySettings;
using UserManagement.Application.CompanySettings.Queries.GetCompanySettings;
using UserManagement.Application.CompanySettings.Queries.GetCompanySettingsById;
using UserManagement.Presentation.Controllers;

namespace UserManagement.UnitTests.Presentation.CompanySettings
{
    public sealed class CompanySettingsControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);

        private CompanySettingsController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task CreateAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateCompanySettingsCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateAsync(new CreateCompanySettingsCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateCompanySettingsCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Update(new UpdateCompanySettingsCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetByIdAsync_WhenFound_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetCompanySettingByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<CompanySettingsDTO> { IsSuccess = true, Data = new CompanySettingsDTO() });

            var result = await CreateSut().GetByIdAsync();

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
