namespace StylekAPI.Helpers;

public class AppSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string UploadPath { get; set; } = "wwwroot/uploads";
    public string AdminEmail { get; set; } = "admin@stylek.com";
    public string AdminPassword { get; set; } = "Admin@123456";
}
