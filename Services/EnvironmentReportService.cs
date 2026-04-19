namespace StudentPortal.Diagnostics.Services;

public class EnvironmentReportService : IEnvironmentReportService
{
    public string GetEnvironmentSummary(IWebHostEnvironment env) =>
        $"🌐 Среда: {env.EnvironmentName} | 📦 Приложение: {env.ApplicationName} | 📁 Контент: {env.ContentRootPath}";
}