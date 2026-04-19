namespace StudentPortal.Diagnostics.Services;

public class RequestCounterService : IRequestCounterService
{
    private int _count;
    public int IncrementAndGet() => System.Threading.Interlocked.Increment(ref _count);
}