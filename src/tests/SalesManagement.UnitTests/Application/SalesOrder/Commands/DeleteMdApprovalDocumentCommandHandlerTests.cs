using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Dtos.Lookups.Users;
using MediatR;
using Microsoft.Extensions.Logging;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.SalesOrder.Commands.DeleteMdApprovalDocument;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.SalesOrder.Commands;

public sealed class DeleteMdApprovalDocumentCommandHandlerTests
{
    private readonly Mock<IFileUploadService> _mockFileUpload = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
    private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
    private readonly Mock<ICompanyLookup> _mockCompanyLookup = new(MockBehavior.Loose);
    private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
    private readonly Mock<ILogger<DeleteMdApprovalDocumentCommandHandler>> _mockLogger = new();

    private DeleteMdApprovalDocumentCommandHandler CreateSut() =>
        new(_mockFileUpload.Object, _mockMediator.Object, _mockIpService.Object,
            _mockCompanyLookup.Object, _mockUnitLookup.Object, _mockLogger.Object);

    private void SetupLookups()
    {
        _mockIpService.Setup(s => s.GetCompanyId()).Returns(1);
        _mockIpService.Setup(s => s.GetUnitId()).Returns(1);

        _mockCompanyLookup.Setup(c => c.GetAllCompanyAsync())
            .ReturnsAsync(new List<CompanyLookupDto>
            {
                new CompanyLookupDto { CompanyId = 1, CompanyName = "TestCompany" }
            });

        _mockUnitLookup.Setup(u => u.GetAllUnitAsync())
            .ReturnsAsync(new List<UnitLookupDto>
            {
                new UnitLookupDto { UnitId = 1, UnitName = "TestUnit" }
            });
    }

    [Fact]
    public async Task Handle_NullFilePath_ReturnsFalse()
    {
        var command = new DeleteMdApprovalDocumentCommand { FilePath = null };

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_EmptyFilePath_ReturnsFalse()
    {
        var command = new DeleteMdApprovalDocumentCommand { FilePath = "" };

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhitespaceFilePath_ReturnsFalse()
    {
        var command = new DeleteMdApprovalDocumentCommand { FilePath = "   " };

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ValidFilePath_CallsDeleteFileAsync()
    {
        SetupLookups();
        _mockFileUpload
            .Setup(f => f.DeleteFileAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        var command = new DeleteMdApprovalDocumentCommand { FilePath = "test-doc.pdf" };

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        _mockFileUpload.Verify(f => f.DeleteFileAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_FileServiceReturnsFalse_ReturnsFalse()
    {
        SetupLookups();
        _mockFileUpload
            .Setup(f => f.DeleteFileAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        var command = new DeleteMdApprovalDocumentCommand { FilePath = "nonexistent.pdf" };

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_SuccessfulDelete_PublishesAuditEvent()
    {
        SetupLookups();
        _mockFileUpload
            .Setup(f => f.DeleteFileAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        var command = new DeleteMdApprovalDocumentCommand { FilePath = "test-doc.pdf" };

        await CreateSut().Handle(command, CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e =>
                    e.ActionCode == "SALESORDER_MDAPPROVAL_DELETE"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_FailedDelete_DoesNotPublishAuditEvent()
    {
        SetupLookups();
        _mockFileUpload
            .Setup(f => f.DeleteFileAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        var command = new DeleteMdApprovalDocumentCommand { FilePath = "nonexistent.pdf" };

        await CreateSut().Handle(command, CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
