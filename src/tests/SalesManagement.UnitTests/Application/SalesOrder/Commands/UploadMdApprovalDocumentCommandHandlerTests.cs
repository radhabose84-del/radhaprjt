using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Dtos.Lookups.Users;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SalesManagement.Application.SalesOrder.Commands.UploadMdApprovalDocument;
using SalesManagement.Application.SalesOrder.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.SalesOrder.Commands;

public sealed class UploadMdApprovalDocumentCommandHandlerTests
{
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
    private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
    private readonly Mock<ICompanyLookup> _mockCompanyLookup = new(MockBehavior.Loose);
    private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
    private readonly Mock<ILogger<UploadMdApprovalDocumentCommandHandler>> _mockLogger = new();

    private UploadMdApprovalDocumentCommandHandler CreateSut() =>
        new(_mockMediator.Object, _mockIpService.Object,
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
    public async Task Handle_NullFile_ThrowsExceptionRules()
    {
        var command = new UploadMdApprovalDocumentCommand { File = null };

        var sut = CreateSut();
        Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ExceptionRules>()
            .WithMessage("*File is required*");
    }

    [Fact]
    public async Task Handle_EmptyFile_ThrowsExceptionRules()
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(0);

        var command = new UploadMdApprovalDocumentCommand { File = mockFile.Object };

        var sut = CreateSut();
        Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ExceptionRules>()
            .WithMessage("*File is required*");
    }

    [Fact]
    public async Task Handle_ValidFile_ReturnsDocumentDto()
    {
        SetupLookups();
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(100);
        mockFile.Setup(f => f.FileName).Returns("test-doc.pdf");
        mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new UploadMdApprovalDocumentCommand { File = mockFile.Object };

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.ImageName.Should().NotBeNullOrWhiteSpace();
        result.ImageName.Should().EndWith(".pdf");
    }

    [Fact]
    public async Task Handle_ValidFile_PublishesAuditEvent()
    {
        SetupLookups();
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(100);
        mockFile.Setup(f => f.FileName).Returns("test-doc.pdf");
        mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new UploadMdApprovalDocumentCommand { File = mockFile.Object };

        await CreateSut().Handle(command, CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e =>
                    e.ActionCode == "SALESORDER_MDAPPROVAL_UPLOAD"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
