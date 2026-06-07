namespace StylekAPI.Helpers;

public static class OrderNumberGenerator
{
    public static string Generate() =>
        $"SK-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(100000, 999999)}";
}
