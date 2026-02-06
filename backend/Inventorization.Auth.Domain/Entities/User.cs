using Inventorization.Base.Models;

namespace Inventorization.Auth.Domain.Entities;

/// <summary>
/// Represents a user in the authentication system
/// </summary>
public class User : BaseEntity
{
    private User() { }  // EF Core only

    /// <summary>
    /// Creates a new user with the provided credentials
    /// </summary>
    public User(string email, string passwordHash, string fullName)
    {
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required", nameof(email));
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("Password hash is required", nameof(passwordHash));
        if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentException("Full name is required", nameof(fullName));

        Id = Guid.NewGuid();
        Email = email;
        PasswordHash = passwordHash;
        FullName = fullName;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string FullName { get; private set; } = null!;
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation properties
    public ICollection<UserRole> UserRoles { get; } = new List<UserRole>();
    public ICollection<RefreshToken> RefreshTokens { get; } = new List<RefreshToken>();

    /// <summary>
    /// Updates user profile information
    /// </summary>
    public void UpdateProfile(string email, string fullName)
    {
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required", nameof(email));
        if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentException("Full name is required", nameof(fullName));
        
        Email = email;
        FullName = fullName;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates user password hash
    /// </summary>
    public void SetPassword(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("Password hash is required", nameof(passwordHash));
        
        PasswordHash = passwordHash;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates the user account
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Activates the user account
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Assigns a role to the user
    /// </summary>
    /// <param name="role">The role to assign</param>
    /// <exception cref="ArgumentNullException">Thrown when role is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when user already has the role</exception>
    public void AssignRole(Role role)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));
        
        if (HasRole(role.Id))
            throw new InvalidOperationException($"User already has role {role.Name}");
        
        UserRoles.Add(new UserRole(Id, role.Id));
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Revokes a role from the user
    /// </summary>
    /// <param name="roleId">The ID of the role to revoke</param>
    /// <exception cref="InvalidOperationException">Thrown when user does not have the role</exception>
    public void RevokeRole(Guid roleId)
    {
        var userRole = UserRoles.FirstOrDefault(ur => ur.RoleId == roleId);
        if (userRole == null)
            throw new InvalidOperationException($"User does not have role {roleId}");
        
        UserRoles.Remove(userRole);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if the user has a specific role
    /// </summary>
    /// <param name="roleId">The ID of the role to check</param>
    /// <returns>True if the user has the role, false otherwise</returns>
    public bool HasRole(Guid roleId) => UserRoles.Any(ur => ur.RoleId == roleId);
}
