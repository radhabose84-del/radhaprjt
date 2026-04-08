using Contracts.Dtos.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.Quotations.QuotationEntry.Commands.Create;
using PurchaseManagement.Application.Quotations.QuotationEntry.Commands.Update;
using PurchaseManagement.Application.Quotations.QuotationEntry.DTOs;
using PurchaseManagement.Application.Quotations.QuotationEntry.Queries.GetAllQuotations;
using PurchaseManagement.Application.Quotations.QuotationEntry.Queries.GetQuotationAutoComplete;
using PurchaseManagement.Application.Quotations.QuotationEntry.Queries.GetQuotationById;
using PurchaseManagement.Presentation.Controllers;

namespace PurchaseManagement.UnitTests.Controllers
{
    public sealed class QuotationEntryControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private QuotationEntryController CreateSut() => new(_mockMediator.Object);

        private static GetQuotationHeaderDto BuildHeaderDto(int id = 1) =>
            new(id, "QT001", 1, "Supplier", 1, "RFQ001", DateOnly.FromDateTime(DateTime.Today),
                1, "Air", 0m, 1, "Net30", 1, "FOB", 0m, 0m, 0m, 0m, 0m, 1, null, null,
                new List<GetQuotationDetailDto>());

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllQuotationsQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<(IReadOnlyList<QuotationListItemDto> Items, int Total)>((new List<QuotationListItemDto>(), 0)));

            var result = await CreateSut().GetAll(1, 20, null, CancellationToken.None);

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_WhenFound_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetQuotationByIdQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(BuildHeaderDto()));

            var result = await CreateSut().GetById(1, CancellationToken.None);

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_WhenNotFound_ReturnsNotFound()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetQuotationByIdQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<GetQuotationHeaderDto>(null!));

            var result = await CreateSut().GetById(999, CancellationToken.None);

            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetQuotationAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<QuotationAutoCompleteDto>());

            var result = await CreateSut().AutoComplete("test", CancellationToken.None);

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsStatusCodeResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateQuotationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var command = new CreateQuotationCommand(1, 1, "QT001", DateOnly.FromDateTime(DateTime.Today),
                null, 0m, null, null, null, null, 0m, 0m, 0m, 0m, new List<QuotationDetailDto>());
            var result = await CreateSut().Create(command, CancellationToken.None);

            result.Should().BeOfType<ObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsStatusCodeResult()
        {
            var command = new UpdateQuotationCommand(1, 1, 1, "QT001", DateOnly.FromDateTime(DateTime.Today),
                1, 0m, 1, 1, 0m, 0m, 0m, 0m, 0m, "", new List<QuotationDetailDto>(), 1);
            var result = await CreateSut().Update(1, command, CancellationToken.None);

            result.Should().BeOfType<ObjectResult>();
        }

        [Fact]
        public async Task Create_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateQuotationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var command = new CreateQuotationCommand(1, 1, "QT001", DateOnly.FromDateTime(DateTime.Today),
                null, 0m, null, null, null, null, 0m, 0m, 0m, 0m, new List<QuotationDetailDto>());
            await CreateSut().Create(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<CreateQuotationCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
