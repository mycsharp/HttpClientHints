// Copyright © myCSharp.de - all rights reserved

namespace MyCSharp.HttpClientHints;

/// <summary>
/// Provides methods to interpret HTTP client hints.
/// </summary>
public static class HttpClientHintsInterpreter
{
    /// <summary>
    /// Determines if the provided hint indicates a mobile device.
    /// </summary>
    /// <param name="mobileHeaderValue">The client hint for mobile status, expected to be "?1" for mobile, "?0" for non-mobile, or null if unknown.</param>
    /// <returns>
    /// A nullable boolean value indicating the mobile status:
    /// <list type="bullet">
    /// <item><c>true</c> if the hint indicates a mobile device ("?1").</item>
    /// <item><c>false</c> if the hint indicates a non-mobile device ("?0").</item>
    /// <item><c>null</c> if the hint does not indicate a specific status.</item>
    /// </list>
    /// </returns>
    public static bool? IsMobile(string? mobileHeaderValue)
    {
        if (mobileHeaderValue is "?1")
        {
            return true;
        }

        if (mobileHeaderValue is "?0")
        {
            return false;
        }

        return null;
    }
}
