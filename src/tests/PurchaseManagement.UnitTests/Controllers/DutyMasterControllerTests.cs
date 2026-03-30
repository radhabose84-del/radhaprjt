using MediatR;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.DutyMaster;
using PurchaseManagement.Application.DutyMaster.Command.Create;
using PurchaseManagement.Application.DutyMaster.Queries.GetAllDutyMaster;
using PurchaseManagement.Application.DutyMaster.Queries.GetDutyMasterAutocomplete;
using PurchaseManagement.Presentation.Controllers;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Controllers
{
    public sealed class DutyMasterControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DutyMasterController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllDutyMasterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(((IReadOnlyList<DutyMasterDto>)new List<DutyMasterDto>(), 0));

            var result = await CreateSut().GetAll(1, 20, null);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllDutyMasterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(((IReadOnlyList<DutyMasterDto>)new List<DutyMasterDto>(), 0));

            await CreateSut().GetAll(1, 20, null);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetAllDutyMasterQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsOkResult()
        {
            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateDutyMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateAsync(DutyMasterBuilders.ValidDto());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateDutyMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            await CreateSut().CreateAsync(DutyMasterBuilders.ValidDto());

            _mockMediator.Verify(
                m => m.Send(It.IsAny<CreateDutyMasterCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<PurchaseManagement.Application.Purchase.DutyMaster.Command.Update.UpdateDutyMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().UpdateAsync(DutyMasterBuilders.ValidDto());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task DeleteAsync_ReturnsOkResult()
        {
            var result = await CreateSut().DeleteAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Autocomplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDutyMasterAutocompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<DutyMasterAutocompleteDto>)new List<DutyMasterAutocompleteDto>());

            var result = await CreateSut().Autocomplete("DC");

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
