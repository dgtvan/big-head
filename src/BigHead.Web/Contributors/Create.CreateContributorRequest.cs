using System.ComponentModel.DataAnnotations;

namespace BigHead.Web.Contributors;

public class CreateContributorRequest
{
  public const string Route = "/Contributors";

  [Required]
  public string Name { get; set; } = String.Empty;
}
