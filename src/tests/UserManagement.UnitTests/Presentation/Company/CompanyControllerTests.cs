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

namespace UserManagement.UnitTests.Presentation.Company
{
    public sealed class CompanyControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);

        private CompanyController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
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

            var result = await CreateSut().GetAllCompaniesAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ValidId_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetCompanyByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetByIdDTO());

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_InvalidId_ReturnsBadRequest()
        {
            var result = await CreateSut().GetByIdAsync(0);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateCompanyCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateAsync(new CreateCompanyCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_WhenFound_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetCompanyByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetByIdDTO { Id = 1 });

            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateCompanyCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Update(new UpdateCompanyCommand
            {
                Company = new UpdateCompanyDTO { Id = 1 }
            });

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_WhenNotFound_ReturnsNotFound()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetCompanyByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetByIdDTO)null!);

            var result = await CreateSut().Update(new UpdateCompanyCommand
            {
                Company = new UpdateCompanyDTO { Id = 999 }
            });

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteCompanyCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Delete(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetCompany_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetCompanyAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CompanyAutoCompleteDTO>());

            var result = await CreateSut().GetCompany("Test");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UploadLogo_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UploadFileCompanyCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetCompanyDTO());

            var result = await CreateSut().UploadLogo(new UploadFileCompanyCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task DeleteLogo_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteFileCompanyCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteLogo(new DeleteFileCompanyCommand());

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
