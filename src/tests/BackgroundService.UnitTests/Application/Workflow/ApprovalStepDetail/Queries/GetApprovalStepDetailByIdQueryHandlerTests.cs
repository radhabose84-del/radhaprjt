using AutoMapper;
using BackgroundService.Application.Workflow.ApprovalStepDetails.Queries.GetApprovalStepDetailById;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalStepDetail;
using MediatR;

namespace BackgroundService.UnitTests.Application.Workflow.ApprovalStepDetail.Queries
{
    public sealed class GetApprovalStepDetailByIdQueryHandlerTests
    {
        private readonly Mock<IApprovalStepDetailQuery> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetApprovalStepDetailByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsDto()
        {
            var entity = new BackgroundService.Domain.Entities.Workflow.ApprovalStepDetail { Id = 1 };
            var dto = new ApprovalStepDetailByIdDto { Id = 1, WorkFlowTypeId = 1, StepOrder = 1 };

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<ApprovalStepDetailByIdDto>(entity))
                .Returns(dto);

            var sut = CreateSut();

            var result = await sut.Handle(
                new GetApprovalStepDetailByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_CallsGetByIdOnce()
        {
            var entity = new BackgroundService.Domain.Entities.Workflow.ApprovalStepDetail { Id = 1 };

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<ApprovalStepDetailByIdDto>(entity))
                .Returns(new ApprovalStepDetailByIdDto { Id = 1 });

            var sut = CreateSut();

            await sut.Handle(new GetApprovalStepDetailByIdQuery { Id = 1 }, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetByIdAsync(1), Times.Once);
        }
    }
}
