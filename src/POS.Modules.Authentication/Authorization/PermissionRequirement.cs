using Microsoft.AspNetCore.Authorization;

namespace POS.Modules.Authentication.Authorization;

public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }
    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}

public static class Policies
{
    public const string CanManageUsers = "CanManageUsers";
    public const string CanApproveRefunds = "CanApproveRefunds";
    public const string CanReprintReceipts = "CanReprintReceipts";
}

