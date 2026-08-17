namespace WpfTemplate.Models;

public sealed class MenuItemModel
{
    public required string Key { get; init; }

    public required string Title { get; init; }

    public required Type ViewModelType { get; init; }
}
