﻿namespace Pekspro.RadioStorm.Sandbox.WPF.Program;

public sealed partial class ProgramControl : UserControl
{
    public ProgramControl()
    {
        InitializeComponent();
    }

    private ProgramModel ViewModel
    {
        get
        {
            var vm = DataContext as ProgramModel;

            return vm;
        }
    }
}
