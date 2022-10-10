namespace Server.Services;

public class DateTimeService : IDateTimeService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DateTimeService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool TryToGetTimeZoneInfoFromCookie(out TimeZoneInfo? timeZoneInfo)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            timeZoneInfo = null;
            return false;
        }

        if (!_httpContextAccessor.HttpContext.Request.Cookies.TryGetValue(
                "timeZone", out string? timeZoneId))
        {
            timeZoneInfo = null;
            return false;
        }

        timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId!);
        return true;
    }
}