using Ardalis.Result;

namespace BigHead.UseCases.Projects.MarkToDoItemComplete;

/// <summary>
/// Create a new Project.
/// </summary>
/// <param name="Name"></param>
public record MarkToDoItemCompleteCommand(int ProjectId, int ToDoItemId) : Ardalis.SharedKernel.ICommand<Result>;
