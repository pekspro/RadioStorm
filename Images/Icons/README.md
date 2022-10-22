# About icons

RadioStorm is using both raster-based (.png) icons and vector-based icons. Most
icons are "designed" with the Segoe MDL2 Assets font from Microsoft. It contains
a lot of
[icons](https://learn.microsoft.com/en-us/windows/apps/design/style/segoe-ui-symbol-font).

## Raster-based icons

Most raster-based icons is stored in the
[Source\Pekspro.RadioStorm.MAUI\Resources\Images](../../Source/Pekspro.RadioStorm.MAUI/Resources/Images/)
folder. Note that these files are SVG-files, but they are automatically rendered
to .png, adopted to different screen densities, by MAUI during compilation. The
image sizes are configured in the
[.csproj-file](../../Source/Pekspro.RadioStorm.MAUI/Pekspro.RadioStorm.MAUI.csproj)
(look for `MauiImage`).

## Vector-based icons

All artwork for vector-based icons are found in this folder. These are used in
the application with the Path element. Read more about the [Path markup syntax
here](https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/shapes/path-markup-syntax).

There is a bit of work that needs to be done to get everything working. In the
SVG-files, the icon has to a single object. Also, no translating or scaling can
be done.

Let's look on [player_play.svg](player_play.svg) as an example. If you open this
in a text editor, you will find this:

    <path
       id="rect846"
       style="fill:#000000;stroke-width:0.132291"
       d="M8,5v14l11,-7z"
       sodipodi:nodetypes="cccccccccc" />

The essential part here is `M8,5v14l11,-7z`. This defines all points etc in the
drawing. Next, look on
[App.xaml](../../Source/Pekspro.RadioStorm.MAUI/App.xaml). Here you find this
code:

    <Style x:Key="PathPlayerPlay" TargetType="Path" BasedOn="{StaticResource PathBase}">
        <Setter Property="Data" Value="M8,5v14l11,-7M 24,24 0,0z" />
    </Style>

This defines the style of a Path element. Note that `Data` property are set to
the same content as in the .SVG-file. Almost, at least. It ends with `M 24,24
0,0z`. This just defines two move instructions to two different points. `24,24`
is the lower right coordinate, and `0,0` is the upper left. This makes sure that
the original canvas size is 24x24 units. If you done have this, MAUI will just
ignore everything outside these coordinates that are used, and the image will
distorted. Therefore, you cannot have anything outside this area.

This style can then be used like this:

    <Path   
        Style="{x:StaticResource PathPlayerPlay}"
        WidthRequest="48"
        />

This will be drawn with 48 units, which is double as the original width. But
since it is vector-based, it will look good anyway.
