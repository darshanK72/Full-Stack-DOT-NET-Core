/*
 * FILE ROLE: Response DTO used by the intro endpoints to demonstrate how
 *            ASP.NET Core serializes C# types to JSON responses.
 * SECTIONS IN THIS FILE:
 *   11. WelcomeMessage — response DTO shape and serialization notes
 */

namespace IntroductionToAspNetCore.Models;

/*
 * SECTION 11: RESPONSE DTO — WelcomeMessage
 * ─────────────────────────────────────────────────────────────────────────────
 * In ASP.NET Core, endpoint handlers return plain C# objects. The framework
 * serializes them to JSON automatically using System.Text.Json (the default
 * serializer since ASP.NET Core 3.0).
 *
 * WHY A SEPARATE DTO?
 *   • Decouples the API contract from internal domain types.
 *   • Controls exactly which fields are exposed in the JSON response.
 *   • Prevents accidental exposure of internal state (e.g., EF navigation
 *     properties, password hashes, internal counters).
 *
 * RECORD vs CLASS for DTOs:
 *   record   — immutable by default; value equality; concise positional syntax;
 *              ideal for read-only response shapes (like this one).
 *   class    — mutable; reference equality; needed when the DTO receives data
 *              from a form or JSON body (requires a default constructor + setters,
 *              or a constructor that the model binder can call).
 *
 * SERIALIZATION BEHAVIOUR (System.Text.Json defaults):
 *   • Serializes public properties only (private fields are ignored).
 *   • A positional record's generated properties are public → serialized.
 *   • Property names are camelCased by default (ASP.NET Core's JsonOptions).
 *
 *   JSON output for GET / with appName="ASP.NET Core Intro":
 *   {
 *     "appName": "ASP.NET Core Intro",
 *     "greeting": "Hello, World! Welcome to ASP.NET Core."
 *   }
 *
 * POSITIONAL RECORD SYNTAX:
 *   public sealed record WelcomeMessage(string AppName, string Greeting);
 *   The compiler generates:
 *     • A primary constructor: WelcomeMessage(string AppName, string Greeting)
 *     • Public init-only properties: AppName and Greeting
 *     • Value equality (Equals, GetHashCode, ==, !=)
 *     • Deconstruct method: Deconstruct(out string appName, out string greeting)
 *
 * COVERED IN DETAIL LATER → 08. Model Binding & Validation
 */
public sealed record WelcomeMessage(
    string AppName,  // application name read from appsettings.json → "AppName" key
    string Greeting  // personalised greeting string produced by GreetingService
);
