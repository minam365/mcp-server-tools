using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace TimeServerTools;

[McpServerToolType]
public static class TimeTools
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="timezoneName"></param>
    /// <returns></returns>
    [McpServerTool, Description("Get the current server time.")]
    public static TimeResult GetCurrentTime(
        [Description("IANA timezone name (e.g., 'America/New_York', 'Europe/London'). Use '{local_tz}' as local timezone if no timezone provided by the user.")] 
        string timezoneName)
    {
        if (string.IsNullOrWhiteSpace(timezoneName) || timezoneName == "{local_tz}")
        {
            var localNow = DateTime.Now;
            return new TimeResult(localNow, TimeZoneInfo.Local.Id, isLocal: true, isDST: TimeZoneInfo.Local.IsDaylightSavingTime(localNow));
        }

        try
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timezoneName);
            var utcNow = DateTime.UtcNow;
            var target = TimeZoneInfo.ConvertTimeFromUtc(utcNow, tz);

            // if caller passed an IANA name, try to return that rather than the Windows ID
            string resultZone = tz.Id;
            if (timezoneName.Contains('/') && TimeZoneInfo.TryConvertWindowsIdToIanaId(tz.Id, out var ianaId))
            {
                resultZone = ianaId;
            }

            return new TimeResult(target, resultZone, isLocal: false, isDST: tz.IsDaylightSavingTime(target));
        }
        catch (TimeZoneNotFoundException)
        {
            var now = DateTime.UtcNow;
            return new TimeResult(now, timezoneName ?? "UTC", isLocal: false, isDST: false);
        }
        catch (InvalidTimeZoneException)
        {
            var now = DateTime.UtcNow;
            return new TimeResult(now, timezoneName ?? "UTC", isLocal: false, isDST: false);
        }
    }

    /// <summary>
    /// Get the server's local timezone.'
    /// </summary>
    /// <param name="timezoneName"></param>
    /// <returns></returns>
    /// <exception cref="McpException"></exception>
    [McpServerTool, Description("Get the server's local timezone.")]
    public static TimeZoneInfo GetLocalTimeZone(
        [Description("This parameter is ignored. Pass any value or leave empty.")] 
        string? timezoneName = null)
    {
        if(string.IsNullOrWhiteSpace(timezoneName) || timezoneName == "{local_tz}")
        {
            return TimeZoneInfo.Local;
        }
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timezoneName);
        }
        catch (TimeZoneNotFoundException)
        {
            throw new McpException($"Timezone '{timezoneName}' not found on this system.");
        }
        catch (InvalidTimeZoneException)
        {
            throw new McpException($"Timezone '{timezoneName}' is invalid.");
        }
        catch (Exception ex)
        {
            throw new McpException($"Error retrieving timezone '{timezoneName}': {ex.Message}");
        }
        
    }
}