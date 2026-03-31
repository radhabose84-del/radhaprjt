using Contracts.Common;
using InventoryManagement.Application.UsageType.Commands.CreateUsageType;
using InventoryManagement.Application.UsageType.Commands.DeleteUsageType;
using InventoryManagement.Application.UsageType.Commands.UpdateUsageType;
using InventoryManagement.Application.UsageType.Dto;
using InventoryManagement.Application.UsageType.Queries.GetAllUsageType;
using InventoryManagement.Application.UsageType.Queries.GetUsageTypeAutoComplete;
using InventoryManagement.Application.UsageType.Queries.GetUsageTypeById;
using InventoryManagement.Presentation.Controllers;
using InventoryManagement.UnitTests.TestData;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.UnitTests.Controllers
{
    public sealed class UsageTypeControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private UsageTypeController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllUsageTypeQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<UsageTypeDto>>
                {
                    IsSuccess = true,
                    Data = new List<UsageTypeDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllUsageTypeAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllUsageTypeQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<UsageTypeDto>>
                {
                    IsSuccess = true,
                    Data = new List<UsageTypeDto>()
                });

            await CreateSut().GetAllUsageTypeAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetAllUsageTypeQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetUsageTypeByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(UsageTypeBuilders.ValidDto());

            var result = await CreateSut().GetUsageTypeByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetUsageTypeAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<UsageTypeLookupDto>)new List<UsageTypeLookupDto>
                {
                    UsageTypeBuilders.ValidLookupDto()
                });

            var result = await CreateSut().GetUsageTypeAutoCompleteAsync("test");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateUsageTypeCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int>
                {
                    IsSuccess = true,
                    Data = 1
                });

            var result = await CreateSut().CreateUsageType(UsageTypeBuilders.ValidCreateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateUsageTypeCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int>
                {
                    IsSuccess = true,
                    Data = 1
                });

            var result = await CreateSut().UpdateUsageType(UsageTypeBuilders.ValidUpdateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteUsageTypeCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteUsageType(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteUsageTypeCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().DeleteUsageType(1);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<DeleteUsageTypeCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
