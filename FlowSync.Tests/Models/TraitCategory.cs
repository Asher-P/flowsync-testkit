namespace FlowSync.Tests.Models;

public class TraitCategory
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int CategoryGroupId { get; set; }
    public TraitCategoryGroup CategoryGroup { get; set; }
    public bool IsActive { get; set; }
}

public class TraitCategoryGroup
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int OptionGroupId { get; set; }
    public int PropertiesId { get; set; }
    public bool AutoCompleteMissingOptions { get; set; }
    public CategoryLogicGroup LogicGroup { get; set; }
    public object Properties { get; set; }
}

public enum CategoryLogicGroup
{
    Range = 1
}
