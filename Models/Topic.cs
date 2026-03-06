using DotNetStudyAssistant.Models.Enums;

namespace DotNetStudyAssistant.Models;

public class Topic : BaseModel
{
    public string Name { get; set; } = string.Empty;
    public CSharpCategory Category { get; set; }
    public string Description { get; set; } = string.Empty;
}
