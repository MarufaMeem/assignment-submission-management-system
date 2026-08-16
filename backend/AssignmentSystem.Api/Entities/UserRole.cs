namespace AssignmentSystem.Api.Entities;

/// <summary>
/// Fixed set of roles. Deliberately an enum, not a lookup table -
/// see README "Assumptions" for why (roles are not dynamic/admin-configurable
/// in this system, so a Roles table would be unused complexity).
/// </summary>
public enum UserRole
{
    Admin = 0,
    Teacher = 1,
    Student = 2
}
