using System.Threading;

public class TaskHandle
{
    private CancellationTokenSource m_cancellationTokenSource;

    public CancellationToken GetNewToken()
    {
        Stop();
        m_cancellationTokenSource = new CancellationTokenSource();
        return m_cancellationTokenSource.Token;
    }

    public void Stop()
    {
        if (m_cancellationTokenSource == null) return;
        m_cancellationTokenSource.Cancel();
        m_cancellationTokenSource.Dispose();
        m_cancellationTokenSource = null;
    }
}