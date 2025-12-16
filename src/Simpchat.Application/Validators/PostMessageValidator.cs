using FluentValidation;
using Simpchat.Application.Models.Messages;
using Simpchat.Application.Validators.Configs;

public class PostMessageValidator : AbstractValidator<PostMessageDto>
{
    public PostMessageValidator()
    {
        // Content is optional (can be empty for file-only messages)
        // Only validate length when content is provided
        RuleFor(m => m.Content)
            .MinimumLength(PostMessageConfig.ContentMinLength)
                .When(m => !string.IsNullOrEmpty(m.Content))
                .WithMessage($"Message content must be at least {PostMessageConfig.ContentMinLength} character")
            .MaximumLength(PostMessageConfig.ContentMaxLength)
                .WithMessage($"Message content max length is {PostMessageConfig.ContentMaxLength} characters");
    }
}