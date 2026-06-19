using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Commands.BulkUpdateMaster;
using FinanceManagement.Application.ScheduleIII.Dto;

namespace FinanceManagement.UnitTests.Application.ScheduleIII.Commands
{
    public sealed class BulkUpdateMasterCommandHandlerTests
    {
        private readonly Mock<IScheduleIIICommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private BulkUpdateMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object);

        private static BulkUpdateMasterCommand ValidCommand() =>
            new()
            {
                Lines = new List<BulkDetailLineUpdate>
                {
                    new() { Id = 1, ScheduleIIISectionId = 1, ScheduleIIISectionItemId = 10, DisplayOrder = 2, IsActive = 0 },
                    new() { Id = 2, ScheduleIIISectionId = 1, ScheduleIIISectionItemId = 11, DisplayOrder = 1, IsActive = 1 }
                }
            };

        [Fact]
        public async Task Handle_MapsIsActive_AndReturnsCount()
        {
            List<FinanceManagement.Domain.Entities.ScheduleIIIDetail>? captured = null;
            _mockCommandRepo
                .Setup(r => r.UpdateDetailRangeAsync(It.IsAny<List<FinanceManagement.Domain.Entities.ScheduleIIIDetail>>()))
                .Callback<List<FinanceManagement.Domain.Entities.ScheduleIIIDetail>>(d => captured = d)
                .ReturnsAsync(2);

            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be(2);
            captured!.Single(x => x.Id == 1).IsActive.Should().Be(Status.Inactive);
            captured!.Single(x => x.Id == 2).IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public async Task Handle_PublishesBulkUpdateAuditEvent()
        {
            _mockCommandRepo
                .Setup(r => r.UpdateDetailRangeAsync(It.IsAny<List<FinanceManagement.Domain.Entities.ScheduleIIIDetail>>()))
                .ReturnsAsync(2);

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "S3_DETAIL_BULK_UPDATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
