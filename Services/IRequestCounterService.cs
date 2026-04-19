namespace StudentPortal.Diagnostics.Services;

public interface IRequestCounterService
{
    int IncrementAndGet();
}