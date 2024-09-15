using FastEndpoints;
using FluentValidation;
using BigHead.Infrastructure.Data.Config;

namespace BigHead.Web.Projects;

/// <summary>
/// See: https://fast-endpoints.com/docs/validation
/// </summary>
public class CreateToDoItemValidator : Validator<CreateToDoItemRequest>
{
  public CreateToDoItemValidator()
  {
    RuleFor(x => x.ProjectId)
      .GreaterThan(0);
    RuleFor(x => x.Title)
      .NotEmpty()
      .MinimumLength(2)
      .MaximumLength(DataSchemaConstants.DEFAULT_NAME_LENGTH);
    RuleFor(x => x.Description)
      .NotEmpty();
  }
}
