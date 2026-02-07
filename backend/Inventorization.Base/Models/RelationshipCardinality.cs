namespace Inventorization.Base.Models;

/// <summary>
/// Defines the cardinality constraint of a relationship
/// </summary>
public enum RelationshipCardinality
{
    /// <summary>
    /// Relationship is optional (nullable foreign key)
    /// Example: Order.ShippingAddressId (can be null if not shipped yet)
    /// </summary>
    Optional,

    /// <summary>
    /// Relationship is required (non-nullable foreign key)
    /// Example: Order.CustomerId (every order must have a customer)
    /// </summary>
    Required
}
