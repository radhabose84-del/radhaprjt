using FluentValidation.TestHelper;
using InventoryManagement.Application.Common.Interfaces.Item.Templates;
using InventoryManagement.Application.Item.Templates.Commands.UpdateTemplate;
using InventoryManagement.Application.Item.Templates.DTOs;
using InventoryManagement.Presentation.Validation.Item.Templates;

namespace InventoryManagement.UnitTests.Validators.Item.Templates
{
    public sealed class UpdateTemplateCommandValidatorTests
    {
        private readonly Mock<ITemplateQueryRepository> _mockRepo = new(MockBehavior.Loose);

        public UpdateTemplateCommandValidatorTests()
        {
            _mockRepo
                .Setup(r => r.ExistsByNameAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
        }

        private UpdateTemplateCommandValidator CreateValidator() => new(_mockRepo.Object);

        private static UpdateTemplateCommand ValidCommand() => new(
            Id: 1,
            TemplateName: "Updated Template",
            Parameters: new List<TemplateParameterDto>
            {
                new TemplateParameterDto { Parameter = "Dimension", Numeric = false }
            },
            IsActive: 1
        );

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = new UpdateTemplateCommand(Id: 0, TemplateName: "Template", Parameters: null, IsActive: 1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyTemplateName_FailsValidation(string? name)
        {
            var command = new UpdateTemplateCommand(Id: 1, TemplateName: name!, Parameters: null, IsActive: 1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicateTemplateName_FailsValidation()
        {
            _mockRepo
                .Setup(r => r.ExistsByNameAsync("Updated Template", 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(ValidCommand());

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_NumericParamMaxLessThanMin_FailsValidation()
        {
            var command = new UpdateTemplateCommand(
                Id: 1,
                TemplateName: "T",
                Parameters: new List<TemplateParameterDto>
                {
                    new TemplateParameterDto { Parameter = "Load", Numeric = true, MinimumValue = 50m, MaximumValue = 10m }
                },
                IsActive: 1
            );

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
