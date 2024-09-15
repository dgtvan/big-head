using Ardalis.Result;
using Ardalis.SharedKernel;

namespace BigHead.UseCases.Projects.ListIncompleteItems;

public record ListIncompleteItemsByProjectQuery(int ProjectId) : IQuery<Result<IEnumerable<ToDoItemDTO>>>;
