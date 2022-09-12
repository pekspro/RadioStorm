namespace Pekspro.RadioStorm.UI.ViewModel.Program;

public sealed record StringIntComboItem(string Description, int Value)
{
    public override string ToString()
    {
        return Description;
    }
}
