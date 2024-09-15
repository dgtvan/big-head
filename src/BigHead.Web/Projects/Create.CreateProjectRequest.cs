using System.ComponentModel.DataAnnotations;

namespace BigHead.Web.Projects;

public class CreateProjectRequest
{
  public const string Route = "/Projects";

  [Required]
  public string? Name { get; set; }
}
