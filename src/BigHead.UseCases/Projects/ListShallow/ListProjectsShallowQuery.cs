using Ardalis.Result;
using Ardalis.SharedKernel;

namespace BigHead.UseCases.Projects.ListShallow;

public record ListProjectsShallowQuery(int? Skip, int? Take) : IQuery<Result<IEnumerable<ProjectDTO>>>;
