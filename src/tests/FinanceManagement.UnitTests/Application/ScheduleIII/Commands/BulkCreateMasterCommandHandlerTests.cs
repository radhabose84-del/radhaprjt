using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Commands.BulkCreateMaster;
using FinanceManagement.Application.ScheduleIII.Dto;

namespace FinanceManagement.UnitTests.Application.ScheduleIII.Commands
{
    public sealed class BulkCreateMasterCommandHandlerTests
    {
        private readonly Mock<IScheduleIIICommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);

        public BulkCreateMasterCommandHandlerTests()
        {
            _mockIp.Setup(x => x.GetCompanyId()).Returns(1);
            _mockIp.Setup(x => x.GetDivisionId()).Returns(7);
        }

        private BulkCreateMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockIp.Object);

        private static BulkCreateMasterCommand ValidCommand() =>
            new()
            {
                Lines = new List<BulkDetailLineInput>
                {
                    new() { ScheduleIIISectionId = 1, ScheduleIIISectionItemId = 10, DisplayOrder = 1 },
                    new() { ScheduleIIISectionId = 1, ScheduleIIISectionItemId = 11, DisplayOrder = 2 }
                }
            };

        [Fact]
        public async Task Handle_EnsuresHeader_AssignsHeaderId_AndReturnsCount()
        {
            List<FinanceManagement.Domain.Entities.ScheduleIIIDetail>? captured = null;
            _mockCommandRepo.Setup(r => r.EnsureHeaderAsync(1, 7)).ReturnsAsync(99);
            _mockCommandRepo
                .Setup(r => r.CreateDetailRangeAsync(It.IsAny<List<FinanceManagement.Domain.Entities.ScheduleIIIDetail>>()))
                .Callback<List<FinanceManagement.Domain.Entities.ScheduleIIIDetail>>(d => captured = d)
                .ReturnsAsync(2);

            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be(2);
            captured.Should().HaveCount(2);
            captured!.Should().OnlyContain(x => x.ScheduleIIIHeaderId == 99);
        }

        [Fact]
        public async Task Handle_PublishesBulkCreateAuditEvent()
        {
            _mockCommandRepo.Setup(r => r.EnsureHeaderAsync(1, 7)).ReturnsAsync(99);
            _mockCommandRepo
                .Setup(r => r.CreateDetailRangeAsync(It.IsAny<List<FinanceManagement.Domain.Entities.ScheduleIIIDetail>>()))
                .ReturnsAsync(2);

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "S3_DETAIL_BULK_CREATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
