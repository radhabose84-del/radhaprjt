using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IGRN.IGRNEntry;
using PurchaseManagement.Application.GRN.GRNEntry.Commands.UpdateGRNEntry;
using PurchaseManagement.Domain.Entities.GRN.GRNEntry;

namespace PurchaseManagement.UnitTests.Application.GRN.GRNEntry.Commands
{
    public sealed class UpdateGRNEntryCommandHandlerTests
    {
        private readonly Mock<IGRNEntryCommandRepository> _mockCmdRepo = new(MockBehavior.Loose);
        private readonly Mock<IGRNEntryQueryRepository> _mockQryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);

        private UpdateGRNEntryCommandHandler CreateSut() =>
            new(_mockCmdRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockQryRepo.Object, _mockIp.Object);

        [Fact]
        public async Task Handle_UpdateFails_ThrowsExceptionRules()
        {
            _mockMapper
                .Setup(m => m.Map<GrnHeader>(It.IsAny<object>()))
                .Returns(new GrnHeader { GrnDetails = new List<GrnDetail>() });

            _mockCmdRepo
                .Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<GrnHeader>(), It.IsAny<List<CalculatedDetail>>(), It.IsAny<List<UpdateGRNEntryDto.UpdateGRNDetailsDto>>()))
                .ReturnsAsync(false);

            var command = new UpdateGRNEntryCommand
            {
                GrnEntryUpdate = new UpdateGRNEntryDto
                {
                    UpdateGRNDetailsDtos = new List<UpdateGRNEntryDto.UpdateGRNDetailsDto>()
                }
            };

            Func<Task> act = () => CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*GRN update failed*");
        }

        [Fact]
        public async Task Handle_SuccessfulUpdate_ReturnsTrue()
        {
            _mockMapper
                .Setup(m => m.Map<GrnHeader>(It.IsAny<object>()))
                .Returns(new GrnHeader { GrnDetails = new List<GrnDetail>() });

            _mockCmdRepo
                .Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<GrnHeader>(), It.IsAny<List<CalculatedDetail>>(), It.IsAny<List<UpdateGRNEntryDto.UpdateGRNDetailsDto>>()))
                .ReturnsAsync(true);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var command = new UpdateGRNEntryCommand
            {
                GrnEntryUpdate = new UpdateGRNEntryDto
                {
                    UpdateGRNDetailsDtos = new List<UpdateGRNEntryDto.UpdateGRNDetailsDto>()
                }
            };

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_SuccessfulUpdate_PublishesAuditEvent()
        {
            _mockMapper
                .Setup(m => m.Map<GrnHeader>(It.IsAny<object>()))
                .Returns(new GrnHeader { GrnDetails = new List<GrnDetail>() });

            _mockCmdRepo
                .Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<GrnHeader>(), It.IsAny<List<CalculatedDetail>>(), It.IsAny<List<UpdateGRNEntryDto.UpdateGRNDetailsDto>>()))
                .ReturnsAsync(true);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var command = new UpdateGRNEntryCommand
            {
                GrnEntryUpdate = new UpdateGRNEntryDto
                {
                    UpdateGRNDetailsDtos = new List<UpdateGRNEntryDto.UpdateGRNDetailsDto>()
                }
            };

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
