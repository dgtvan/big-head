using Ardalis.Result;
using Ardalis.SharedKernel;
using BigHead.Core.ProjectAggregate;
using BigHead.Core.ProjectAggregate.Specifications;

namespace BigHead.UseCases.Projects.AddToDoItem;

public class AddToDoItemHandler : ICommandHandler<AddToDoItemCommand, Result<int>>
{
  private readonly IRepository<Project> _repository;

  public AddToDoItemHandler(IRepository<Project> repository)
  {
    _repository = repository;
  }

  public async Task<Result<int>> Handle(AddToDoItemCommand request,
    CancellationToken cancellationToken)
  {
    var spec = new ProjectByIdWithItemsSpec(request.ProjectId);
    var entity = await _repository.FirstOrDefaultAsync(spec, cancellationToken);
    if (entity == null)
    {
      return Result.NotFound();
    }

    var newItem = new ToDoItem()
    {
      Title = request.Title!,
      Description = request.Description!
    };

    if(request.ContributorId.HasValue)
    {
      newItem.AddContributor(request.ContributorId.Value);
    }
    entity.AddItem(newItem);
    await _repository.UpdateAsync(entity);

    return Result.Success(newItem.Id);
  }
}
