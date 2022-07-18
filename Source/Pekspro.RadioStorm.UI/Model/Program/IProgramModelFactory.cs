namespace Pekspro.RadioStorm.UI.Model.Program;

public interface IProgramModelFactory
{
    ProgramModel Create(ProgramData programData);
}