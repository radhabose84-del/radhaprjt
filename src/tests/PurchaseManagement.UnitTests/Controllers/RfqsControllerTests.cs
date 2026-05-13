using Contracts.Dtos.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.Quotation.RfqEntry.Commands.Create;
using PurchaseManagement.Application.Quotation.RfqEntry.Commands.DeleteAttachment;
using PurchaseManagement.Application.Quotation.RfqEntry.Commands.UploadAttachment;
using PurchaseManagement.Application.Quotation.RfqEntry.Commands.UpsertDraft;
using PurchaseManagement.Application.Quotation.RfqEntry.Dtos;
using PurchaseManagement.Application.Quotation.RfqEntry.DTOs;
using PurchaseManagement.Application.Quotation.RfqEntry.Queries.GetAllRfq;
using PurchaseManagement.Application.Quotation.RfqEntry.Queries.GetRfqAutoComplete;
using PurchaseManagement.Application.Quotation.RfqEntry.Queries.GetRfqAutoCompleteComparison;
using PurchaseManagement.Application.Quotation.RfqEntry.Queries.GetRfqById;
using PurchaseManagement.Presentation.Controllers;

namespace PurchaseManagement.UnitTests.Controllers
{
    public sealed class RfqsControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private RfqsController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task Create_ReturnsCreatedStatusCode()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateRfqCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().Create(new CreateRfqCommand(), CancellationToken.None);

            result.Should().BeOfType<ObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            var result = await CreateSut().Update(new UpdateRfqCommand(), CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetRfqByIdQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new RfqDto(
                    1, null, "RFQ001", 1, "Approved", 1, "Manual", null,
                    DateOnly.FromDateTime(DateTime.Today),
                    Array.Empty<RfqItemDto>(),
                    Array.Empty<RfqSupplierDto>(),
                    Array.Empty<RfqAttachmentDto>())));

            var result = await CreateSut().GetById(1, false, CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllRfqQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<(IReadOnlyList<RfqListItemDto> Items, int Total)>((new List<RfqListItemDto>(), 0)));

            var result = await CreateSut().GetAll(null, 1, 15, null, CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateDraft_ReturnsCreatedStatusCode()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpsertRfqDraftCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new UpsertRfqDraftResult(1, "RFQ001")));

            var result = await CreateSut().CreateDraft(new UpsertRfqDraftCommand(), CancellationToken.None);

            result.Should().BeOfType<ObjectResult>();
        }

        [Fact]
        public async Task UpdateDraft_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpsertRfqDraftCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new UpsertRfqDraftResult(1, "RFQ001")));

            var result = await CreateSut().UpdateDraft(new UpsertRfqDraftCommand(), CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetRfqAutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetRfqAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RfqAutoCompleteDto>());

            var result = await CreateSut().GetRfqAutoComplete("test", null, CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task ApprovedComparison_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetRfqAutoCompleteComparisonQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RfqAutoCompleteDto>());

            var result = await CreateSut().ApprovedComparison("test", null, null, CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateRfqCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            await CreateSut().Create(new CreateRfqCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<CreateRfqCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UploadAttachment_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UploadRfqAttachmentCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UploadRfqAttachmentResultDto("TEMP_x.pdf", "specs.pdf", 1024, "application/pdf"));

            var result = await CreateSut().UploadAttachment(new UploadRfqAttachmentCommand(), CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UploadAttachment_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UploadRfqAttachmentCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UploadRfqAttachmentResultDto("TEMP_x.pdf", "x.pdf", 1, null));

            await CreateSut().UploadAttachment(new UploadRfqAttachmentCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<UploadRfqAttachmentCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteAttachment_HandlerReturnsTrue_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteRfqAttachmentCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteAttachment(1, 7, CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task DeleteAttachment_HandlerReturnsFalse_ReturnsNotFoundResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteRfqAttachmentCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await CreateSut().DeleteAttachment(1, 9999, CancellationToken.None);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task DeleteAttachment_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteRfqAttachmentCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().DeleteAttachment(1, 7, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<DeleteRfqAttachmentCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
