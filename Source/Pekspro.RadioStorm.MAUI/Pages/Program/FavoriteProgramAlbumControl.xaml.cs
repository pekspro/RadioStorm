namespace Pekspro.RadioStorm.MAUI.Pages.Program;

public sealed partial class FavoriteProgramAlbumControl
{
    public FavoriteProgramAlbumControl()
    {
        InitializeComponent();
    }

    private ProgramModel ViewModel => (ProgramModel) BindingContext;
}
