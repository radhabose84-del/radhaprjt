using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IComplaint;
using SalesManagement.Application.Complaint.Commands.DeleteComplaint;
using SalesManagement.Presentation.Validation.Complaint;

namespace SalesManagement.UnitTests.Validators.Complaint;

public sealed class DeleteComplaintCommandValidatorTests
{
    private readonly Mock<IComplaintQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

    private DeleteComplaintCommandValidator CreateValidator() =>
        new(_mockQueryRepo.Object);

    [Fact]
    public async Task Validate_ValidCommand_PassesValidation()
    {
        _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);

        var result = await CreateValidator().TestValidateAsync(new DeleteComplaintCommand(1));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_ZeroId_FailsValidation()
    {
        _mockQueryRepo.Setup(r => r.NotFoundAsync(0)).ReturnsAsync(true);

        var result = await CreateValidator().TestValidateAsync(new DeleteComplaintCommand(0));

        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public async Task Validate_NotFoundId_FailsValidation()
    {
        _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);

        var result = await CreateValidator().TestValidateAsync(new DeleteComplaintCommand(99));

        result.ShouldHaveValidationErrorFor(x => x.Id);
    }
}
