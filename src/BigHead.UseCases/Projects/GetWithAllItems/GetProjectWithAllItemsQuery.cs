using Ardalis.Result;
using Ardalis.SharedKernel;

namespace BigHead.UseCases.Projects.GetWithAllItems;

public record GetProjectWithAllItemsQuery(int ProjectId) : IQuery<Result<ProjectWithAllItemsDTO>>;
