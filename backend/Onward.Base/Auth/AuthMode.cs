namespace Onward.Base.Auth;

/// <summary>
/// Defines the authorization mode used by a consumer service.
/// </summary>
public enum AuthMode
{
    /// <summary>
    /// Stateless JWT validation only.
    /// Tokens are valid until expiry; revocation is not enforced in real-time.
    /// </summary>
    Local,

    /// <summary>
    /// JWT validation + real-time introspection against the Auth Service.
    /// Supports immediate token revocation, user block/unblock, and per-JTI caching.
    /// </summary>
    Online
}
