namespace Pekspro.RadioStorm.UI.ViewModel.Program;

public record StringIntComboItem(string Description, int Value)
{
    public override string ToString()
    {
        return Description;
    }
}
