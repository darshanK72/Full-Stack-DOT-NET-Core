using System;

namespace SqlConnectionAndConnectionStrings.Utils;

/*
 * FILE ROLE:
 *   Security guidance for storing connection strings outside source code.
 *
 * SECTIONS IN THIS FILE:
 *   8. Connection string security — config, not source code
 */

/*
 * =========================================================================
 * SECTION 8: CONNECTION STRING SECURITY — CONFIG, NOT SOURCE CODE
 * =========================================================================
 *
 * Never hardcode production passwords or API keys in Program.cs or commit them
 * to git. Patterns:
 *
 *   appsettings.json     "ConnectionStrings": { "Default": "Server=…;…" }
 *   environment variables ConnectionStrings__Default (double underscore)
 *   User Secrets (dev)   dotnet user-secrets set "ConnectionStrings:Default" "…"
 *   Azure Key Vault / managed identity in cloud deployments
 *
 * This console tutorial has no appsettings — the preview below shows the shape
 * you will use in ASP.NET Core modules. Replace *** with secrets from tooling.
 *
 *   // ASP.NET Core (preview — full module later):
 *   // var cs = builder.Configuration.GetConnectionString("Default");
 *   // using var conn = new SqlConnection(cs);
 *
 * Integrated Security avoids SQL passwords on domain-joined machines but still
 * needs least-privilege SQL logins or Windows groups in production.
 * -------------------------------------------------------------------------
 */
public static class ConnectionSecurityPreview
{
    public static void PrintSecurityGuidance()
    {
        Console.WriteLine("--- Connection string security ---");
        Console.WriteLine("  • Store strings in configuration or secret stores — not in source.");
        Console.WriteLine("  • User Secrets: dotnet user-secrets set \"ConnectionStrings:Default\" \"Server=…\"");
        Console.WriteLine("  • Environment:  ConnectionStrings__Default=Server=…");
        Console.WriteLine("  • TrustServerCertificate=true is for local dev only.");
        Console.WriteLine("  • Use Encrypt=true against production and Azure SQL.");
    }
}
