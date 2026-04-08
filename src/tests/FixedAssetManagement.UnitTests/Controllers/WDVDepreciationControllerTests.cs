using FAM.Application.WDVDepreciation.Commands.CreateDepreciation;
using FAM.Application.WDVDepreciation.Commands.DeleteDepreciation;
using FAM.Application.WDVDepreciation.Commands.LockDepreciation;
using FAM.Application.WDVDepreciation.Queries.GetDepreciation;
using FAM.Presentation.Controllers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FixedAssetManagement.UnitTests.Controllers
{
    public sealed class WDVDepreciationControllerTests
    {
        private readonly Mock<ISender> _mockSender = new(MockBehavior.Strict);

        private WDVDepreciationController CreateSut() => new(_mockSender.Object);

        [Fact]
        public async Task GetDepreciation_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetDepreciationQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CalculationDepreciationDto>
                {
                    new CalculationDepreciationDto { FinYearId = 1 }
                });

            var request = new GetDepreciationQuery { FinYearId = 1 };
            var result = await CreateSut().GetDepreciationAsync(request);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetDepreciation_CallsMediatorSend_Once()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetDepreciationQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CalculationDepreciationDto>());

            var request = new GetDepreciationQuery { FinYearId = 1 };
            await CreateSut().GetDepreciationAsync(request);

            _mockSender.Verify(
                m => m.Send(It.IsAny<GetDepreciationQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<CreateDepreciationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((CalculationDepreciationDto?)null!);

            var command = new CreateDepreciationCommand { FinYearId = 1 };
            var result = await CreateSut().CreateAsync(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_CallsMediatorSend_Once()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<CreateDepreciationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((CalculationDepreciationDto?)null!);

            var command = new CreateDepreciationCommand { FinYearId = 1 };
            await CreateSut().CreateAsync(command);

            _mockSender.Verify(
                m => m.Send(It.IsAny<CreateDepreciationCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<DeleteDepreciationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CalculationDepreciationDto());

            var command = new DeleteDepreciationCommand { FinYearId = 1 };
            var result = await CreateSut().DeleteAsync(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSend_Once()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<DeleteDepreciationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CalculationDepreciationDto());

            var command = new DeleteDepreciationCommand { FinYearId = 1 };
            await CreateSut().DeleteAsync(command);

            _mockSender.Verify(
                m => m.Send(It.IsAny<DeleteDepreciationCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task LockDepreciation_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<LockDepreciationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CalculationDepreciationDto());

            var command = new LockDepreciationCommand { FinYearId = 1 };
            var result = await CreateSut().LockDepreciationAsync(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task LockDepreciation_CallsMediatorSend_Once()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<LockDepreciationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CalculationDepreciationDto());

            var command = new LockDepreciationCommand { FinYearId = 1 };
            await CreateSut().LockDepreciationAsync(command);

            _mockSender.Verify(
                m => m.Send(It.IsAny<LockDepreciationCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
