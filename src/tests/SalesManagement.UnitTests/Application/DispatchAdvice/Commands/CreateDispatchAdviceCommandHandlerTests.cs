using SalesManagement.Domain.Entities;
using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDispatchAdvice;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.DispatchAdvice.Commands.CreateDispatchAdvice;

using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.DispatchAdvice.Commands
{
    public sealed class CreateDispatchAdviceCommandHandlerTests
    {
        private readonly Mock<IDispatchAdviceCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IDispatchAdviceQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterQueryRepository> _mockMiscRepo = new(MockBehavior.Loose);
        private readonly Mock<IDocumentSequenceLookup> _mockDocSeqLookup = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateDispatchAdviceCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMiscRepo.Object,
                _mockDocSeqLookup.Object, _mockIpService.Object,
                _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int newId = 1)
        {
            _mockMapper
                .Setup(m => m.Map<DispatchAdviceHeader>(It.IsAny<CreateDispatchAdviceCommand>()))
                .Returns(new DispatchAdviceHeader());

            _mockMiscRepo
                .Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SalesManagement.Domain.Entities.MiscMaster { Id = 1 });

            _mockIpService.Setup(s => s.GetUnitId()).Returns(1);

            _mockDocSeqLookup
                .Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(10);

            _mockDocSeqLookup
                .Setup(d => d.GenerateDocumentNumber(It.IsAny<int>()))
                .ReturnsAsync(new List<string> { "DA0001" });

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<DispatchAdviceHeader>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(newId);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(new CreateDispatchAdviceCommand(), CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(newId: 42);
            var result = await CreateSut().Handle(new CreateDispatchAdviceCommand(), CancellationToken.None);

            result.Data.Should().Be(42);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(new CreateDispatchAdviceCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "DISPATCHADVICE_CREATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NoTransactionType_ThrowsExceptionRules()
        {
            _mockMapper
                .Setup(m => m.Map<DispatchAdviceHeader>(It.IsAny<CreateDispatchAdviceCommand>()))
                .Returns(new DispatchAdviceHeader());

            _mockMiscRepo
                .Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SalesManagement.Domain.Entities.MiscMaster { Id = 1 });

            _mockIpService.Setup(s => s.GetUnitId()).Returns(1);

            _mockDocSeqLookup
                .Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync((int?)null);

            Func<Task> act = async () => await CreateSut().Handle(new CreateDispatchAdviceCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>();
        }

        [Fact]
        public async Task Handle_ValidCommand_PassesDispatchTypeNameToRepo()
        {
            _mockMapper.Setup(m => m.Map<DispatchAdviceHeader>(It.IsAny<CreateDispatchAdviceCommand>()))
                .Returns(new DispatchAdviceHeader());
            _mockMiscRepo.Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SalesManagement.Domain.Entities.MiscMaster { Id = 1 });
            _mockMiscRepo.Setup(r => r.GetByIdAsync(7))
                .ReturnsAsync(new SalesManagement.Application.MiscMaster.Dto.MiscMasterDto { Id = 7, Description = "DirectToParty" });
            _mockIpService.Setup(s => s.GetUnitId()).Returns(1);
            _mockDocSeqLookup.Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(10);
            _mockDocSeqLookup.Setup(d => d.GenerateDocumentNumber(It.IsAny<int>()))
                .ReturnsAsync(new List<string> { "DA0001" });
            _mockCommandRepo.Setup(r => r.CreateAsync(
                It.IsAny<DispatchAdviceHeader>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(1);

            await CreateSut().Handle(new CreateDispatchAdviceCommand { DispatchTypeId = 7 }, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.CreateAsync(
                It.IsAny<DispatchAdviceHeader>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<int>(), "DirectToParty", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NullDispatchType_PassesNullDispatchTypeNameToRepo()
        {
            _mockMapper.Setup(m => m.Map<DispatchAdviceHeader>(It.IsAny<CreateDispatchAdviceCommand>()))
                .Returns(new DispatchAdviceHeader());
            _mockMiscRepo.Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SalesManagement.Domain.Entities.MiscMaster { Id = 1 });
            _mockMiscRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((SalesManagement.Application.MiscMaster.Dto.MiscMasterDto?)null);
            _mockIpService.Setup(s => s.GetUnitId()).Returns(1);
            _mockDocSeqLookup.Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(10);
            _mockDocSeqLookup.Setup(d => d.GenerateDocumentNumber(It.IsAny<int>()))
                .ReturnsAsync(new List<string> { "DA0001" });
            _mockCommandRepo.Setup(r => r.CreateAsync(
                It.IsAny<DispatchAdviceHeader>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(1);

            await CreateSut().Handle(new CreateDispatchAdviceCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(r => r.CreateAsync(
                It.IsAny<DispatchAdviceHeader>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<int>(), (string?)null, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_SetsDispatchNoAndUnitIdOnEntity()
        {
            var entity = new DispatchAdviceHeader();
            _mockMapper.Setup(m => m.Map<DispatchAdviceHeader>(It.IsAny<CreateDispatchAdviceCommand>()))
                .Returns(entity);
            _mockMiscRepo.Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SalesManagement.Domain.Entities.MiscMaster { Id = 1 });
            _mockIpService.Setup(s => s.GetUnitId()).Returns(9);
            _mockDocSeqLookup.Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(10);
            _mockDocSeqLookup.Setup(d => d.GenerateDocumentNumber(It.IsAny<int>()))
                .ReturnsAsync(new List<string> { "DA/2026/00099" });
            _mockCommandRepo.Setup(r => r.CreateAsync(
                It.IsAny<DispatchAdviceHeader>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(1);

            await CreateSut().Handle(new CreateDispatchAdviceCommand(), CancellationToken.None);

            entity.UnitId.Should().Be(9);
            entity.DispatchNo.Should().Be("DA/2026/00099");
        }

        [Fact]
        public async Task Handle_NoUnitIdFromJwt_FallsBackToSalesOrderUnitId()
        {
            _mockMapper.Setup(m => m.Map<DispatchAdviceHeader>(It.IsAny<CreateDispatchAdviceCommand>()))
                .Returns(new DispatchAdviceHeader());
            _mockMiscRepo.Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SalesManagement.Domain.Entities.MiscMaster { Id = 1 });
            _mockIpService.Setup(s => s.GetUnitId()).Returns((int?)null);
            _mockQueryRepo.Setup(r => r.GetSalesOrderUnitIdAsync(It.IsAny<int>()))
                .ReturnsAsync(55);
            _mockDocSeqLookup.Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), 55))
                .ReturnsAsync(10);
            _mockDocSeqLookup.Setup(d => d.GenerateDocumentNumber(It.IsAny<int>()))
                .ReturnsAsync(new List<string> { "DA0001" });
            _mockCommandRepo.Setup(r => r.CreateAsync(
                It.IsAny<DispatchAdviceHeader>(), 55, It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(1);

            await CreateSut().Handle(new CreateDispatchAdviceCommand { SalesOrderId = 100 }, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetSalesOrderUnitIdAsync(100), Times.Once);
            _mockCommandRepo.Verify(r => r.CreateAsync(
                It.IsAny<DispatchAdviceHeader>(), 55, It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Handle_RepoThrows_PropagatesException_DoesNotPublishAudit()
        {
            _mockMapper.Setup(m => m.Map<DispatchAdviceHeader>(It.IsAny<CreateDispatchAdviceCommand>()))
                .Returns(new DispatchAdviceHeader());
            _mockMiscRepo.Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SalesManagement.Domain.Entities.MiscMaster { Id = 1 });
            _mockIpService.Setup(s => s.GetUnitId()).Returns(1);
            _mockDocSeqLookup.Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(10);
            _mockDocSeqLookup.Setup(d => d.GenerateDocumentNumber(It.IsAny<int>()))
                .ReturnsAsync(new List<string> { "DA0001" });
            _mockCommandRepo.Setup(r => r.CreateAsync(
                It.IsAny<DispatchAdviceHeader>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("DB write failed"));

            var act = async () => await CreateSut().Handle(new CreateDispatchAdviceCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("DB write failed");

            _mockMediator.Verify(m => m.Publish(
                It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
