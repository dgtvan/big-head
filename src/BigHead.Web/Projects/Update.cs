using BigHead.Core.ProjectAggregate;
using Ardalis.SharedKernel;
using Microsoft.AspNetCore.Mvc;
//using Swashbuckle.AspNetCore.Annotations;
using BigHead.Web.Endpoints.ProjectEndpoints;
using FastEndpoints;
using BigHead.Web.Contributors;
using MediatR;
using Ardalis.Result;
using BigHead.UseCases.Contributors.Update;
using BigHead.UseCases.Projects.Update;

namespace BigHead.Web.Projects;

public class Update : Endpoint<UpdateProjectRequest, UpdateProjectResponse>
{
  private readonly IMediator _mediator;

  public Update(IMediator mediator)
  {
    _mediator = mediator;
  }

  public override void Configure()
  {
    Put(UpdateProjectRequest.Route);
    AllowAnonymous();
  }


  public override async Task HandleAsync(
  UpdateProjectRequest request,
  CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(new UpdateProjectCommand(request.Id, request.Name!));

    if (result.Status == ResultStatus.NotFound)
    {
      await SendNotFoundAsync(cancellationToken);
      return;
    }

    if (result.IsSuccess)
    {
      var dto = result.Value;
      Response = new UpdateProjectResponse(new ProjectRecord(dto.Id, dto.Name));
      return;
    }
  }
}
