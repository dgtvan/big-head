using Ardalis.Result;
using Ardalis.SharedKernel;

namespace BigHead.UseCases.Projects.Delete;

public record DeleteProjectCommand(int ProjectId) : ICommand<Result>;
