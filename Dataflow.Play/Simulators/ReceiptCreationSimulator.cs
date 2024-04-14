using System.Timers;

namespace Dataflow.Play.Simulators;

public class ReceiptCreationSimulator: IDisposable
{
    private System.Timers.Timer timer;
    private int interval; // in milliseconds

    public event EventHandler Elapsed;

    public ReceiptCreationSimulator(int interval)
    {
        this.interval = interval;
        timer = new System.Timers.Timer(interval);
        timer.Elapsed += TimerElapsed;
        timer.AutoReset = true;
    }

    public void Start()
    {
        timer.Start();
    }

    public void Stop()
    {
        timer.Stop();
    }

    private void TimerElapsed(object sender, ElapsedEventArgs e)
    {
        OnElapsed(EventArgs.Empty);
    }

    protected virtual void OnElapsed(EventArgs e)
    {
        Elapsed?.Invoke(this, e);
    }

    public void Dispose()
    {
        timer.Elapsed -= TimerElapsed;
    }
}
