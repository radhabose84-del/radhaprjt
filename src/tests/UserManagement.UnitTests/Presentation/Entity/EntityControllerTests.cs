using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Entity.Commands.CreateEntity;
using UserManagement.Application.Entity.Commands.DeleteEntity;
using UserManagement.Application.Entity.Commands.UpdateEntity;
using UserManagement.Application.Entity.Queries.GetCompanyBasedUnit;
using UserManagement.Application.Entity.Queries.GetEntity;
using UserManagement.Application.Entity.Queries.GetEntityAutoComplete;
using UserManagement.Application.Entity.Queries.GetEntityBasedCompany;
using UserManagement.Application.Entity.Queries.GetEntityById;
using UserManagement.Application.Entity.Queries.GetEntityLastCode;
using UserManagement.Presentation.Controllers;

namespace UserManagement.UnitTests.Presentation.Entity
{
    public sealed class EntityControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<EntityController>> _mockLogger = new();

        private EntityController CreateSut() =>
            new(_mockMediator.Object, _mockLogger.Object);

        [Fact]
        public async Task GetAll_WithData_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetEntityQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetEntityDTO>>
                {
                    IsSuccess = true,
                    Data = new List<GetEntityDTO> { new() },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllEntityAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ValidId_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetEntityByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetEntityDTO());

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
        public async Task GenerateEntityCode_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetEntityLastCodeQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<string> { IsSuccess = true, Data = "ENT001" });

            var result = await CreateSut().GenerateEntityCodeAsync();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateEntityCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateAsync(new CreateEntityCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateEntityCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().UpdateAsync(new UpdateEntityCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task DeleteEntityAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteEntityCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().DeleteEntityAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetEntity_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetEntityAutocompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<EntityAutoCompleteDto>());

            var result = await CreateSut().GetEntity("Test");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetCompaniesAsync_ValidId_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetEntityBasedCompanyQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetEntityBasedCompanyDto> { new() });

            var result = await CreateSut().GetCompaniesAsync(1, CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetUnitsBasedCompanies_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetCompanyBasedUnitQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetCompanyBasedUnitDto> { new() });

            var result = await CreateSut().GetUnitsBasedCompanies("1,2");

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
