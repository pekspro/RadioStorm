namespace Pekspro.RadioStorm.MAUI.Pages.Program;

public partial class FavoriteProgramControl
{
    public FavoriteProgramControl()
    {
        InitializeComponent();

        WidthStateHelper.ConfigureWidthState(this);
    }

    protected ProgramModel ViewModel => (ProgramModel) BindingContext;
}
