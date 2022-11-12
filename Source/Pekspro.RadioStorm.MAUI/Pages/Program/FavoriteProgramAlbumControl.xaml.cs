namespace Pekspro.RadioStorm.MAUI.Pages.Program;

public sealed partial class FavoriteProgramAlbumControl
{
    public FavoriteProgramAlbumControl()
    {
        InitializeComponent();
    }

    private ProgramModel ViewModel => (ProgramModel) BindingContext;

    private void AlbumItem_SizeChanged(object sender, EventArgs e)
    {
        GridLargeMediaButton.WidthRequest = LargeMediaButton.WidthRequest = Width - 16;
    }
}
