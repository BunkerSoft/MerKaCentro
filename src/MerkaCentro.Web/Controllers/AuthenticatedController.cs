using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MerkaCentro.Web.Controllers;

[Authorize]
public abstract class AuthenticatedController : Controller
{
    /// <summary>
    /// Retrieves the current user's ID from the authentication claims.
    /// Throws when the claim is missing or not a valid GUID; callers can rely
    /// on [Authorize] being applied to avoid unauthenticated access.
    /// </summary>
    protected Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            // This should not happen in normal operation because [Authorize] is applied,
            // but fail fast with a clear message if the claim is absent or invalid.
            throw new InvalidOperationException("Unable to determine current user ID. Ensure the user is authenticated.");
        }

        return userId;
    }
}
