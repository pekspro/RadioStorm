namespace Pekspro.RadioStorm.Utilities;

public interface IMainThreadTimer : IDisposable
{
    public Guid Id { get; }
    bool AutoReset { get; set; }
    bool Enabled { get; set; }
    double Interval { get; set; }

    void SetupCallBack(Action action);
    void Start();
    void Stop();
}