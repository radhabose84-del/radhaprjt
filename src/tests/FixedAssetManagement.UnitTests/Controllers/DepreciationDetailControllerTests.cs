using Contracts.Common;
using FAM.Application.DepreciationDetail.Commands.CreateDepreciationDetail;
using FAM.Application.DepreciationDetail.Queries.GetDepreciationDetail;
using FAM.Presentation.Controllers;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FixedAssetManagement.UnitTests.Controllers
{
    public sealed class DepreciationDetailControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);

        private DepreciationDetailController CreateSut() =>
            new(_mockMediator.Object);

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateDepreciationDetailCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("Depreciation created successfully.");

            var result = await CreateSut().CreateAsync(DepreciationDetailBuilders.ValidCreateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateDepreciationDetailCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("Depreciation created successfully.");

            await CreateSut().CreateAsync(DepreciationDetailBuilders.ValidCreateCommand());

            _mockMediator.Verify(m => m.Send(It.IsAny<CreateDepreciationDetailCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
