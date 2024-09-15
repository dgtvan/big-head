using Ardalis.Result;
using Ardalis.SharedKernel;

namespace BigHead.UseCases.Projects.Update;

public record UpdateProjectCommand(int ProjectId, string NewName) : ICommand<Result<ProjectDTO>>;
