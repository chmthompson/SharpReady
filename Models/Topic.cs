using SharpReady.Models.Enums;

namespace SharpReady.Models;

public class Topic : BaseModel
{
    public string Name { get; set; } = string.Empty;
    public CSharpCategory Category { get; set; }
    public string Description { get; set; } = string.Empty;
}
