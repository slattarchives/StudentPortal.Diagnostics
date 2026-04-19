namespace StudentPortal.Diagnostics.Services;

public class AppVersionService : IAppVersionService
{
    public string GetVersion() => "1.0.0";
    public string GetBuildInfo() => $"Target: .NET 10 | Runtime: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}";
}