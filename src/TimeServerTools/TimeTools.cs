using System;
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
        if (string.IsNullOrWhiteSpace(timezoneName) || timezoneName.Equals("{local_tz}", StringComparison.InvariantCultureIgnoreCase))
        {
            var localNow = DateTime.Now;
            return new TimeResult(localNow, TimeZoneInfo.Local.Id, IsLocal: true, IsDST: TimeZoneInfo.Local.IsDaylightSavingTime(localNow));
        }

        try
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timezoneName);
            var utcNow = DateTime.UtcNow;
            var target = TimeZoneInfo.ConvertTimeFromUtc(utcNow, tz);

            // if caller passed an IANA name, try to return that rather than the Windows ID
            var resultZone = tz.Id;
            if (timezoneName.Contains('/') && TimeZoneInfo.TryConvertWindowsIdToIanaId(tz.Id, out var ianaId))
            {
                resultZone = ianaId;
            }

            return new TimeResult(target, resultZone, IsLocal: false, IsDST: tz.IsDaylightSavingTime(target));
        }
        catch (TimeZoneNotFoundException)
        {
            var now = DateTime.UtcNow;
            return new TimeResult(now, timezoneName ?? "UTC", IsLocal: false, IsDST: false);
        }
        catch (InvalidTimeZoneException)
        {
            var now = DateTime.UtcNow;
            return new TimeResult(now, timezoneName ?? "UTC", IsLocal: false, IsDST: false);
        }
    }

    /// <summary>
    /// Get the local timezone.
    /// </summary>
    /// <param name="timezoneName"></param>
    /// <returns></returns>
    /// <exception cref="McpException"></exception>
    [McpServerTool, Description("Get the server's local timezone.")]
    public static TimeZoneInfo GetLocalTimeZone(
        [Description("This parameter is ignored. Pass any value or leave empty.")] 
        string? timezoneName = null)
    {
        return GetTimeZoneInternal(timezoneName);        
    }

    /// <summary>
    /// Gets the timezone information for the specified timezone name.
    /// </summary>
    /// <param name="timezoneName">The name of the timezone whose detailed information to get.
    /// 
    /// </param>
    /// <returns></returns>
    /// <exception cref="McpException"></exception>
    private static TimeZoneInfo GetTimeZoneInternal(string? timezoneName = null)
    {
        if(string.IsNullOrWhiteSpace(timezoneName) || timezoneName == "{local_tz}")
        {
            return TimeZoneInfo.Local;
        }

        if (timezoneName == "{utc}")
        {
            return TimeZoneInfo.Utc;
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