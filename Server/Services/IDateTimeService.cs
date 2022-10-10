namespace Server.Services;

public interface IDateTimeService
{
    bool TryToGetTimeZoneInfoFromCookie(out TimeZoneInfo? timeZoneInfo);
}