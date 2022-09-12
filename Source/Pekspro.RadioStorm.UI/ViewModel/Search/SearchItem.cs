namespace Pekspro.RadioStorm.UI.ViewModel.Search;

public sealed record SearchItem (SearchItemType Type, int Id, string Title, string? Details, ImageLink? ImageLink, string? TitleColor = null)
{
    public override string ToString()
    {
        return Title;
    }
}
