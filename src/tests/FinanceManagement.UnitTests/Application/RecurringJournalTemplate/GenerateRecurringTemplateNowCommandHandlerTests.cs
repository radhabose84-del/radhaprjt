using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IRecurringGeneration;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IRecurringJournalTemplate;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Commands.GenerateRecurringTemplateNow;
using MediatR;

namespace FinanceManagement.UnitTests.Application.RecurringJournalTemplate
{
    public sealed class GenerateRecurringTemplateNowCommandHandlerTests
    {
        private readonly Mock<IRecurringJournalGenerationService> _service = new(MockBehavior.Strict);
        private readonly Mock<IRecurringJournalTemplateQueryRepository> _templateQuery = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _ip = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mediator = new(MockBehavior.Loose);

        private GenerateRecurringTemplateNowCommandHandler CreateSut() =>
            new(_service.Object, _templateQuery.Object, _ip.Object, _mediator.Object);

        private void SetupApprovedTemplate(int templateId, string statusCode = "Approved") =>
            _templateQuery.Setup(r => r.GetScheduleInfoAsync(templateId))
                .ReturnsAsync(new RecurringTemplateScheduleInfoDto { Id = templateId, StatusCode = statusCode });

        [Fact]
        public async Task Handle_Generated_ReturnsSuccessWithJournalId()
        {
            _ip.Setup(s => s.GetCompanyId()).Returns(1);
            SetupApprovedTemplate(5);
            // generate-now passes autoPost: false; the period is resolved inside the service from the voucher date.
            _service.Setup(s => s.GenerateForTemplateAsync(1, 5, new DateOnly(2026, 7, 15), It.IsAny<CancellationToken>()))
                .ReturnsAsync(99);

            var result = await CreateSut().Handle(new GenerateRecurringTemplateNowCommand
            {
                TemplateId = 5, VoucherDate = new DateOnly(2026, 7, 15)
            }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be(99);
        }

        [Fact]
        public async Task Handle_NothingGenerated_ReturnsFailure()
        {
            _ip.Setup(s => s.GetCompanyId()).Returns(1);
            SetupApprovedTemplate(5);
            _service.Setup(s => s.GenerateForTemplateAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            var result = await CreateSut().Handle(new GenerateRecurringTemplateNowCommand { TemplateId = 5, VoucherDate = new DateOnly(2026, 7, 15) }, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Data.Should().Be(0);
        }

        [Fact]
        public async Task Handle_TemplateNotApproved_Blocked_NoGeneration()
        {
            _ip.Setup(s => s.GetCompanyId()).Returns(1);
            SetupApprovedTemplate(5, statusCode: "Pending");   // template is Pending → must not generate

            var result = await CreateSut().Handle(new GenerateRecurringTemplateNowCommand { TemplateId = 5, VoucherDate = new DateOnly(2026, 7, 15) }, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("not approved");
            result.Data.Should().Be(0);
            // The generation service is never called for a non-approved template (Strict mock → no setup needed).
            _service.Verify(s => s.GenerateForTemplateAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_TemplateNotFound_Blocked()
        {
            _ip.Setup(s => s.GetCompanyId()).Returns(1);
            _templateQuery.Setup(r => r.GetScheduleInfoAsync(It.IsAny<int>())).ReturnsAsync((RecurringTemplateScheduleInfoDto?)null);

            var result = await CreateSut().Handle(new GenerateRecurringTemplateNowCommand { TemplateId = 9, VoucherDate = new DateOnly(2026, 7, 15) }, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("not found");
            _service.Verify(s => s.GenerateForTemplateAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_NoCompany_Throws()
        {
            _ip.Setup(s => s.GetCompanyId()).Returns((int?)null);

            Func<Task> act = async () => await CreateSut().Handle(new GenerateRecurringTemplateNowCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>();
        }
    }
}
