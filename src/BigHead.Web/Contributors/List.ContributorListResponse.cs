using BigHead.Web.Contributors;

namespace BigHead.Web.Contributors;

public class ContributorListResponse
{
  public List<ContributorRecord> Contributors { get; set; } = new();
}
