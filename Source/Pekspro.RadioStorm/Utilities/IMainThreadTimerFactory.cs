namespace Pekspro.RadioStorm.Utilities;

public interface IMainThreadTimerFactory
{
    IMainThreadTimer CreateTimer(string name);
}
