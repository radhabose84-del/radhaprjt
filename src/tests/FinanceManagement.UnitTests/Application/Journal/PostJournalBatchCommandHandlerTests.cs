using Contracts.Common;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Application.JournalMaster.Journal.Commands.PostJournal;
using FinanceManagement.Application.JournalMaster.Journal.Commands.PostJournalBatch;
using MediatR;

namespace FinanceManagement.UnitTests.Application.Journal
{
    public sealed class PostJournalBatchCommandHandlerTests
    {
        private readonly Mock<IMediator> _mediator = new(MockBehavior.Strict);

        private PostJournalBatchCommandHandler CreateSut() => new(_mediator.Object);

        private void SetupPost(int id, string voucherNo) =>
            _mediator.Setup(m => m.Send(It.Is<PostJournalCommand>(c => c.Id == id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<PostJournalResultDto>
                {
                    IsSuccess = true,
                    Message = "Journal voucher posted successfully.",
                    Data = new PostJournalResultDto { JournalId = id, VoucherNo = voucherNo }
                });

        [Fact]
        public async Task Handle_AllSucceed_ReturnsAllPosted()
        {
            SetupPost(3, "JV/2026-27/0001");
            SetupPost(4, "JV/2026-27/0002");

            var result = await CreateSut().Handle(
                new PostJournalBatchCommand { Ids = new() { 3, 4 } }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(2);
            result.Data.Should().OnlyContain(r => r.IsSuccess);
            result.Data!.Select(r => r.VoucherNo).Should().BeEquivalentTo(new[] { "JV/2026-27/0001", "JV/2026-27/0002" });
            result.Message.Should().Contain("2 voucher(s) posted, 0 failed");
        }

        [Fact]
        public async Task Handle_OneFails_OthersStillPosted()
        {
            SetupPost(3, "JV/2026-27/0001");
            _mediator.Setup(m => m.Send(It.Is<PostJournalCommand>(c => c.Id == 5), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ExceptionRules("Journal voucher not found."));

            var result = await CreateSut().Handle(
                new PostJournalBatchCommand { Ids = new() { 3, 5 } }, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();                 // not all succeeded
            result.Data.Should().HaveCount(2);
            result.Data!.Single(r => r.JournalId == 3).IsSuccess.Should().BeTrue();

            var failed = result.Data!.Single(r => r.JournalId == 5);
            failed.IsSuccess.Should().BeFalse();
            failed.Message.Should().Contain("not found");
            result.Message.Should().Contain("1 voucher(s) posted, 1 failed");
        }

        [Fact]
        public async Task Handle_DuplicateIds_PostedOnce()
        {
            SetupPost(3, "JV/2026-27/0001");

            var result = await CreateSut().Handle(
                new PostJournalBatchCommand { Ids = new() { 3, 3 } }, CancellationToken.None);

            result.Data.Should().HaveCount(1);
            _mediator.Verify(m => m.Send(It.Is<PostJournalCommand>(c => c.Id == 3), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
