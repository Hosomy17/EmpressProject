using System.Threading;

public class TaskHandle
{
    private CancellationTokenSource m_cts;

    public CancellationToken GetNewToken()
    {
        Stop();
        m_cts = new CancellationTokenSource();
        return m_cts.Token;
    }

    public void Stop()
    {
        if (m_cts == null) return;
        m_cts.Cancel();
        m_cts.Dispose();
        m_cts = null;
    }
}