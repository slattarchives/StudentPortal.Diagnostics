namespace StudentPortal.Diagnostics.Services;

public interface IAppVersionService
{
    string GetVersion();
    string GetBuildInfo();
}