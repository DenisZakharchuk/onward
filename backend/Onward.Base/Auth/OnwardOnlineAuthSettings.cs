namespace Onward.Base.Auth;

/// <summary>
/// Configuration for the online (real-time introspection) auth mode.
/// Bind from the <c>"OnlineAuth"</c> configuration section.
/// </summary>
public sealed class OnwardOnlineAuthSettings
{
    public const string SectionName = "OnlineAuth";

    /// <summary>
    /// Base URL of the Auth Service (e.g. <c>http://auth-service:5012</c>).
    /// The introspection endpoint will be appended automatically.
    /// </summary>
    public string AuthServiceBaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// How long (in seconds) a successful introspection result is cached per JTI.
    /// Default: 30.  Set to 0 to disable caching (per-request introspection).
    /// </summary>
    public int CacheTtlSeconds { get; set; } = 30;

    /// <summary>
    /// When <c>true</c> (availability-first), requests are allowed through if the Auth
    /// Service is unreachable. When <c>false</c> (security-first, default), requests are
    /// rejected with 401 if the Auth Service cannot be reached.
    /// </summary>
    public bool FailOpen { get; set; } = false;

    /// <summary>
    /// Transport protocol used to call the Auth Service.
    /// Supported values: <c>"Http"</c> (default), <c>"Grpc"</c>.
    /// </summary>
    public string Transport { get; set; } = "Http";

    /// <summary>
    /// Timeout in seconds for each introspection call. Default: 5.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 5;
}
