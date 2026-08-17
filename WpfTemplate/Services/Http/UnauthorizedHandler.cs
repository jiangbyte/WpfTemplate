using WpfTemplate.Services.Abstractions;

namespace WpfTemplate.Services.Http;

public sealed class UnauthorizedHandler : IUnauthorizedHandler
{
    private readonly ISessionStore _session;
    private int _handling;

    public UnauthorizedHandler(ISessionStore session)
    {
        _session = session;
    }

    public void HandleUnauthorized()
    {
        if (Interlocked.Exchange(ref _handling, 1) == 1)
        {
            return;
        }

        try
        {
            _session.Clear();
        }
        finally
        {
            Interlocked.Exchange(ref _handling, 0);
        }
    }
}
