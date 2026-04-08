using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Entity.Commands.CreateEntity;
using UserManagement.Application.Entity.Commands.DeleteEntity;
using UserManagement.Application.Entity.Commands.UpdateEntity;
using UserManagement.Application.Entity.Queries.GetEntity;
using UserManagement.Application.Entity.Queries.GetEntityAutoComplete;
using UserManagement.Application.Entity.Queries.GetEntityBasedCompany;
using UserManagement.Application.Entity.Queries.GetEntityById;
using UserManagement.Application.Entity.Queries.GetEntityLastCode;
using UserManagement.Application.Entity.Queries.GetCompanyBasedUnit;
using UserManagement.Presentation.Controllers;

namespace UserManagement.UnitTests.Controllers
{
    public sealed class EntityControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<EntityController>> _mockLogger = new();

        private EntityController CreateSut() =>
            new(_mockMediator.Object, _mockLogger.Object);

        // --- GetAllEntityAsync ---

        [Fact]
        public async Task GetAllEntityAsync_WithData_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetEntityQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetEntityDTO>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<GetEntityDTO> { new GetEntityDTO() },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllEntityAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllEntityAsync_EmptyData_ReturnsNotFound()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetEntityQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetEntityDTO>>
                {
                    IsSuccess = true,
                    Message = "No records",
                    Data = new List<GetEntityDTO>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllEntityAsync(1, 10);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        // --- GetByIdAsync ---

        [Fact]
        public async Task GetByIdAsync_ValidId_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetEntityByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetEntityDTO());

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetByIdAsync_InvalidId_ReturnsBadRequest()
        {
            var result = await CreateSut().GetByIdAsync(0);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        // --- GenerateEntityCodeAsync ---

        [Fact]
        public async Task GenerateEntityCodeAsync_Success_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetEntityLastCodeQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<string>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = "ENT001"
                });

            var result = await CreateSut().GenerateEntityCodeAsync();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GenerateEntityCodeAsync_Failure_ReturnsBadRequest()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetEntityLastCodeQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = "Failed",
                    Data = null
                });

            var result = await CreateSut().GenerateEntityCodeAsync();

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        // --- CreateAsync ---

        [Fact]
        public async Task CreateAsync_ReturnsOkResult()
        {
            var command = new CreateEntityCommand { EntityName = "TestEntity" };

            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateEntityCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateAsync(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        // --- UpdateAsync ---

        [Fact]
        public async Task UpdateAsync_ReturnsOkResult()
        {
            var command = new UpdateEntityCommand { Id = 1, EntityName = "Updated" };

            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateEntityCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().UpdateAsync(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        // --- DeleteEntityAsync ---

        [Fact]
        public async Task DeleteEntityAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteEntityCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().DeleteEntityAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        // --- GetEntity (AutoComplete) ---

        [Fact]
        public async Task GetEntity_AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetEntityAutocompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<EntityAutoCompleteDto>());

            var result = await CreateSut().GetEntity("test");

            result.Should().BeOfType<OkObjectResult>();
        }

        // --- GetCompaniesAsync ---

        [Fact]
        public async Task GetCompaniesAsync_ValidId_WithData_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetEntityBasedCompanyQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetEntityBasedCompanyDto> { new GetEntityBasedCompanyDto() });

            var result = await CreateSut().GetCompaniesAsync(1, CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetCompaniesAsync_InvalidId_ReturnsBadRequest()
        {
            var result = await CreateSut().GetCompaniesAsync(0, CancellationToken.None);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetCompaniesAsync_NoData_ReturnsNotFound()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetEntityBasedCompanyQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetEntityBasedCompanyDto>());

            var result = await CreateSut().GetCompaniesAsync(1, CancellationToken.None);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        // --- GetUnitsBasedCompanies ---

        [Fact]
        public async Task GetUnitsBasedCompanies_ValidIds_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetCompanyBasedUnitQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetCompanyBasedUnitDto> { new GetCompanyBasedUnitDto() });

            var result = await CreateSut().GetUnitsBasedCompanies("1,2");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetUnitsBasedCompanies_EmptyIds_ReturnsBadRequest()
        {
            var result = await CreateSut().GetUnitsBasedCompanies("");

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetUnitsBasedCompanies_NoData_ReturnsNotFound()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetCompanyBasedUnitQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetCompanyBasedUnitDto>());

            var result = await CreateSut().GetUnitsBasedCompanies("1");

            result.Should().BeOfType<NotFoundObjectResult>();
        }
    }
}
