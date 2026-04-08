using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.Companies.Commands.CreateCompany;
using UserManagement.Application.Companies.Commands.DeleteCompany;
using UserManagement.Application.Companies.Commands.DeleteFileCompany;
using UserManagement.Application.Companies.Commands.UpdateCompany;
using UserManagement.Application.Companies.Commands.UploadFileCompany;
using UserManagement.Application.Companies.Queries.GetCompanies;
using UserManagement.Application.Companies.Queries.GetCompanyAutoComplete;
using UserManagement.Application.Companies.Queries.GetCompanyById;
using UserManagement.Presentation.Controllers;

namespace UserManagement.UnitTests.Controllers
{
    public sealed class CompanyControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);

        private CompanyController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAllCompaniesAsync_ReturnsOkResult()
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetCompanyQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetCompanyDTO>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<GetCompanyDTO> { new GetCompanyDTO() },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            var sut = CreateSut();

            // Act
            var result = await sut.GetAllCompaniesAsync(1, 10);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllCompaniesAsync_CallsMediatorSend_Once()
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetCompanyQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetCompanyDTO>>
                {
                    IsSuccess = true,
                    Data = new List<GetCompanyDTO>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var sut = CreateSut();

            // Act
            await sut.GetAllCompaniesAsync(1, 10);

            // Assert
            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetCompanyQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ValidId_ReturnsOkResult()
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetCompanyByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetByIdDTO());

            var sut = CreateSut();

            // Act
            var result = await sut.GetByIdAsync(1);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetByIdAsync_InvalidId_ReturnsBadRequest()
        {
            // Arrange
            var sut = CreateSut();

            // Act
            var result = await sut.GetByIdAsync(0);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_ReturnsOkResult()
        {
            // Arrange
            var command = new CreateCompanyCommand();
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateCompanyCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var sut = CreateSut();

            // Act
            var result = await sut.CreateAsync(command);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ExistingCompany_ReturnsOkResult()
        {
            // Arrange
            var command = new UpdateCompanyCommand { Company = new UpdateCompanyDTO { Id = 1 } };
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetCompanyByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetByIdDTO());
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateCompanyCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var sut = CreateSut();

            // Act
            var result = await sut.Update(command);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_NonExistingCompany_ReturnsNotFound()
        {
            // Arrange
            var command = new UpdateCompanyCommand { Company = new UpdateCompanyDTO { Id = 999 } };
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetCompanyByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetByIdDTO?)null);

            var sut = CreateSut();

            // Act
            var result = await sut.Update(command);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteCompanyCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var sut = CreateSut();

            // Act
            var result = await sut.Delete(1);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetCompany_ReturnsOkResult()
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetCompanyAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CompanyAutoCompleteDTO>());

            var sut = CreateSut();

            // Act
            var result = await sut.GetCompany("test");

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UploadLogo_ReturnsOkResult()
        {
            // Arrange
            var command = new UploadFileCompanyCommand();
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UploadFileCompanyCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetCompanyDTO());

            var sut = CreateSut();

            // Act
            var result = await sut.UploadLogo(command);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task DeleteLogo_ReturnsOkResult()
        {
            // Arrange
            var command = new DeleteFileCompanyCommand();
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteFileCompanyCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var sut = CreateSut();

            // Act
            var result = await sut.DeleteLogo(command);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
