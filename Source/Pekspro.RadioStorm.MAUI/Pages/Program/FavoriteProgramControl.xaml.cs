namespace Pekspro.RadioStorm.MAUI.Pages.Program;

public sealed partial class FavoriteProgramControl
{
    public FavoriteProgramControl()
    {
        InitializeComponent();

        WidthStateHelper.ConfigureWidthState(this);
    }

    private ProgramModel ViewModel => (ProgramModel) BindingContext;
}
