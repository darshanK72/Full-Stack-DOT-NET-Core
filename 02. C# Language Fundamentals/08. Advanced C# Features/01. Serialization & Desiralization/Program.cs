/*
 * =============================================================================
 * 01. SERIALIZATION & DESERIALIZATION — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: Converting in-memory object graphs into storable or transmittable
 *        formats (JSON, XML, legacy binary) and reconstructing them on the
 *        other side — including attributes, serializer options, and pitfalls.
 *
 * WHY IT MATTERS:
 *   REST APIs, config files, message queues, and caches all move data across
 *   process or machine boundaries. Serialization turns a Person object into JSON
 *   in an HTTP body, persists settings between app restarts, and lets services
 *   written in different languages share a wire format. The serializer you pick
 *   affects security, versioning strategy, and interoperability.
 *
 * WHAT YOU WILL LEARN:
 *   1.  Serialization vocabulary — wire format, round trip, object graph
 *   2.  [Serializable] — legacy binary marker vs modern serializers
 *   3.  System.Text.Json — Serialize, Deserialize, UTF-8 bytes, streams
 *   4.  JsonSerializerOptions — naming, indentation, case, depth, cycles
 *   5.  JSON attributes — JsonPropertyName, JsonIgnore, JsonInclude, order
 *   6.  JSON versioning — missing/extra members, enum wire names
 *   7.  Custom JsonConverter — controlling how one type maps to JSON
 *   8.  JsonNode — reading/writing JSON without a fixed C# type
 *   9.  XmlSerializer — elements, XmlRoot, XmlAttribute, XmlIgnore
 *   10. XmlSerializer requirements and common runtime failures
 *   11. BinaryFormatter (legacy) — why it is obsolete and unsafe
 *   12. Choosing a format — JSON vs XML vs modern binary alternatives
 *
 * =============================================================================
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace SerializationAndDeserialization;

/*
 * =========================================================================
 * SECTION 1: SERIALIZATION BASICS — VOCABULARY
 * =========================================================================
 *
 * Serialization converts a live object graph into bytes or text (the wire
 * format). Deserialization reads that payload and builds new object instances.
 *
 *   Object graph  ──serialize──►  wire format (JSON / XML / bytes)
 *   wire format   ──deserialize►  new object graph (new instances)
 *
 * Deserialization never resurrects the original heap objects — it creates
 * fresh instances populated from the payload. That matters for identity,
 * events, and anything tied to a specific object reference.
 *
 * This chapter uses an alumni roster (filtered to YearOfBirth >= 1999) as the
 * running example across JSON and XML demos in Main.
 * -------------------------------------------------------------------------
 */

/*
 * =========================================================================
 * SECTION 2: THE PERSON MODEL — ATTRIBUTES FOR JSON AND XML
 * =========================================================================
 *
 * System.Text.Json and XmlSerializer ignore [Serializable]. That attribute
 * only mattered for BinaryFormatter and other legacy binary APIs (Section 11).
 *
 * XmlSerializer requirements (checked at runtime when you construct the
 * serializer or call Serialize/Deserialize):
 *
 *   Requirement               | Person satisfies?
 *   --------------------------|------------------------------------------
 *   Public parameterless ctor | Yes (compiler-generated default)
 *   Public read/write props   | Yes (Name, Institute, …)
 *   Only parameterized ctor   | No — XmlSerializer would fail
 *
 * [JsonPropertyName("nickname")] maps a C# property to a specific JSON name
 * regardless of PropertyNamingPolicy. [XmlElement("Nickname")] does the same
 * for XML element names.
 *
 * Attribute.IsDefined (used in Main) is a lightweight preview of reflection —
 * COVERED IN DETAIL LATER → 02. Reflection & Attributes.
 * -------------------------------------------------------------------------
 */
[Serializable]
public sealed class Person
{
    public string Name { get; set; } = string.Empty;
    public string Institute { get; set; } = string.Empty;
    public string Occupation { get; set; } = string.Empty;
    public int YearOfBirth { get; set; }

    /*
     * v2 field: older JSON without "nickname" still deserializes — missing
     * nullable members become null (Section 6).
     */
    [JsonPropertyName("nickname")]
    [XmlElement("Nickname")]
    public string? Nickname { get; set; }

    public override string ToString() =>
        $"{Name} ({Institute}, born {YearOfBirth})";
}

/*
 * =========================================================================
 * SECTION 3: JSON ATTRIBUTES — IGNORE SENSITIVE OR COMPUTED MEMBERS
 * =========================================================================
 *
 * [JsonIgnore] omits a property from JSON output and skips it on read.
 * Use it for secrets, derived values, or navigation properties you never
 * want on the wire.
 *
 * [JsonInclude] (not shown here) can force a non-public field/property into
 * JSON when you opt in — useful for immutable types with private backing
 * fields. COVERED IN DETAIL LATER → 02. Reflection & Attributes for
 * discovering attributes at runtime.
 * -------------------------------------------------------------------------
 */
public sealed class MemberProfile
{
    public string Username { get; set; } = string.Empty;

    [JsonIgnore]
    public string PasswordHash { get; set; } = string.Empty; // never written to JSON

    [JsonIgnore]
    public string DisplayLabel => $"{Username} (member)";  // computed — omit from wire
}

/*
 * =========================================================================
 * SECTION 4: ENUMS ON THE WIRE — JsonStringEnumConverter
 * =========================================================================
 *
 * By default System.Text.Json writes enum values as numbers (0, 1, 2).
 * APIs usually prefer stable string names ("Active", "Alumni").
 *
 * Apply [JsonConverter(typeof(JsonStringEnumConverter))] on the enum or on
 * the containing property. Combine with [JsonPropertyName] on enum members
 * when the wire name differs from the C# identifier.
 * -------------------------------------------------------------------------
 */
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MembershipTier
{
    Guest,
    [JsonPropertyName("full_member")]
    FullMember,
    Alumni
}

public sealed class TieredMember
{
    public string Name { get; set; } = string.Empty;

    [JsonPropertyOrder(1)]
    public MembershipTier Tier { get; set; } // serialized as string via converter
}

/*
 * =========================================================================
 * SECTION 5: CUSTOM JsonConverter — ISO DATE STRINGS
 * =========================================================================
 *
 * When the default mapping is wrong, implement JsonConverter<T> and attach
 * it with [JsonConverter(typeof(...))] on the type or property.
 *
 * Read(ref Utf8JsonReader, …) parses incoming JSON; Write(Utf8JsonWriter, …)
 * emits JSON. Register on JsonSerializerOptions.Converters to apply globally.
 * -------------------------------------------------------------------------
 */
public sealed class IsoDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? text = reader.GetString();
        return DateTime.Parse(text!); // ISO 8601 round-trip from "O" format
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("O")); // 2024-06-01T12:00:00.0000000Z
    }
}

public sealed class EventRegistration
{
    public string EventName { get; set; } = string.Empty;

    [JsonConverter(typeof(IsoDateTimeConverter))]
    public DateTime RegisteredAt { get; set; }
}

/*
 * =========================================================================
 * SECTION 6: XML SHAPE — XmlRoot, XmlElement, XmlAttribute
 * =========================================================================
 *
 * XmlSerializer reflects public properties. Attributes control XML layout:
 *
 *   Attribute      | Effect
 *   ---------------|---------------------------------------------------
 *   [XmlRoot]      | Root element name and optional namespace
 *   [XmlElement]   | Child element name (Person list items)
 *   [XmlAttribute] | Serialize property as XML attribute, not child element
 *   [XmlIgnore]    | Skip property (same idea as JsonIgnore)
 *
 * Serialize AlumniRoster — not List<Person> directly — when you need a
 * named root element and metadata attributes on the document.
 * -------------------------------------------------------------------------
 */
[XmlRoot("AlumniRoster", Namespace = "http://example.edu/alumni")]
public sealed class AlumniRoster
{
    [XmlAttribute("exported")]
    public DateTime ExportedOn { get; set; }

    [XmlElement("Person")]
    public List<Person> Members { get; set; } = new List<Person>();
}

/*
 * =========================================================================
 * SECTION 7: CIRCULAR REFERENCES — ReferenceHandler.IgnoreCycles
 * =========================================================================
 *
 * Parent/child graphs create cycles (Parent.Children → Child, Child.Parent
 * → Parent). Default JSON serialization throws JsonException on cycles.
 *
 * ReferenceHandler.IgnoreCycles replaces repeated object references with null
 * on second visit — good enough for logging and many DTO snapshots. For
 * full graph preservation use ReferenceHandler.Preserve (adds $id/$ref metadata).
 * -------------------------------------------------------------------------
 */
public sealed class OrgUnit
{
    public string Name { get; set; } = string.Empty;
    public List<OrgUnit> Children { get; set; } = new List<OrgUnit>();
    public OrgUnit? Parent { get; set; } // creates a cycle when wired both ways
}

public static class SerializationHelpers
{
    /*
     * =========================================================================
     * SECTION 8: JSON FILE AND UTF-8 HELPERS
     * =========================================================================
     *
     * JsonSerializer.Serialize / Deserialize work on strings, UTF-8 bytes,
     * and streams. ASP.NET Core often uses UTF-8 bytes directly for responses.
     *
     * SerializeToUtf8Bytes avoids an intermediate string allocation — useful
     * for caches and high-throughput APIs.
     * -------------------------------------------------------------------------
     */
    public static void WriteJsonToFile<T>(T value, string path, JsonSerializerOptions options)
    {
        string json = JsonSerializer.Serialize(value, options);
        File.WriteAllText(path, json, Encoding.UTF8);
    }

    public static T? ReadJsonFromFile<T>(string path, JsonSerializerOptions options)
    {
        string json = File.ReadAllText(path, Encoding.UTF8);
        return JsonSerializer.Deserialize<T>(json, options);
    }

    public static byte[] SerializeToUtf8Bytes<T>(T value, JsonSerializerOptions options)
    {
        return JsonSerializer.SerializeToUtf8Bytes(value, options);
    }

    public static T? DeserializeFromUtf8Bytes<T>(ReadOnlySpan<byte> utf8Json, JsonSerializerOptions options)
    {
        return JsonSerializer.Deserialize<T>(utf8Json, options);
    }

    /*
     * =========================================================================
     * SECTION 9: XML FILE HELPERS
     * =========================================================================
     *
     * XmlSerializer is constructed for a concrete type (AlumniRoster here).
     * Reuse one serializer instance per type in production — construction
     * reflects over properties and caches a serialization plan.
     * -------------------------------------------------------------------------
     */
    public static void WriteXmlToFile<T>(T value, string path)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(T));
        using FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write);
        serializer.Serialize(stream, value);
    }

    public static T? ReadXmlFromFile<T>(string path) where T : class
    {
        XmlSerializer serializer = new XmlSerializer(typeof(T));
        using FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        object? result = serializer.Deserialize(stream);
        return result as T;
    }

    public static List<Person> CreateAlumniRoster()
    {
        return new List<Person>
        {
            new Person { Name = "Darshan", Institute = "DYP", Occupation = "Engineer", YearOfBirth = 2000 },
            new Person { Name = "Suyesh", Institute = "KKW", Occupation = "Engineer", YearOfBirth = 1999 },
            new Person { Name = "Dipak", Institute = "DYP", Occupation = "Engineer", YearOfBirth = 2001 },
            new Person { Name = "Parth", Institute = "DYP", Occupation = "Engineer", YearOfBirth = 2002 },
            new Person { Name = "Suyog", Institute = "DYP", Occupation = "Engineer", YearOfBirth = 1998 }
        };
    }

    public static List<Person> FilterFrom1999(IReadOnlyList<Person> source)
    {
        List<Person> filtered = new List<Person>();
        foreach (Person person in source)
        {
            if (person.YearOfBirth >= 1999)
            {
                filtered.Add(person);
            }
        }
        return filtered;
    }

    public static void PrintAlumni(IReadOnlyList<Person> people)
    {
        foreach (Person person in people)
        {
            Console.WriteLine($"  {person.Name} — {person.Institute} ({person.YearOfBirth})");
        }
    }
}

public class Program
{
    /*
     * =========================================================================
     * SECTION 10: DEMONSTRATION — Main orchestrates the chapter demo
     * =========================================================================
     *
     * Main wires the running example: roster filter, JSON/XML round trips,
     * options, versioning, JsonNode, and format summary. Concept blocks live
     * above types and helpers — not duplicated here.
     * -------------------------------------------------------------------------
     */
    public static void Main(string[] args)
    {
        List<Person> alumni = SerializationHelpers.CreateAlumniRoster();
        List<Person> recentGraduates = SerializationHelpers.FilterFrom1999(alumni);

        Console.WriteLine("=== Alumni roster (YearOfBirth >= 1999) ===");
        SerializationHelpers.PrintAlumni(recentGraduates);

        // --- Section 2: [Serializable] is present but ignored by JSON/XML ---
        Console.WriteLine();
        Console.WriteLine("=== [Serializable] on Person ===");
        bool hasSerializable = Attribute.IsDefined(typeof(Person), typeof(SerializableAttribute));
        Console.WriteLine($"Person carries [Serializable]: {hasSerializable} (legacy binary only)");

        // --- Sections 8–8b: JsonSerializerOptions for readable camelCase JSON ---
        JsonSerializerOptions jsonOptions = BuildTutorialJsonOptions();

        string jsonPath = Path.Combine(Path.GetTempPath(), "serialization-demo-alumni.json");
        SerializationHelpers.WriteJsonToFile(recentGraduates, jsonPath, jsonOptions);
        string jsonOnDisk = File.ReadAllText(jsonPath, Encoding.UTF8);

        Console.WriteLine();
        Console.WriteLine("=== JSON on disk (excerpt) ===");
        Console.WriteLine(jsonOnDisk.Length > 240 ? jsonOnDisk[..240] + "..." : jsonOnDisk);

        List<Person>? fromJson = SerializationHelpers.ReadJsonFromFile<List<Person>>(jsonPath, jsonOptions)
            ?? new List<Person>();

        Console.WriteLine();
        Console.WriteLine("=== Round-trip from JSON file ===");
        SerializationHelpers.PrintAlumni(fromJson);

        // --- UTF-8 bytes path (no intermediate string) ---
        byte[] utf8Payload = SerializationHelpers.SerializeToUtf8Bytes(recentGraduates, jsonOptions);
        List<Person>? fromUtf8 = SerializationHelpers.DeserializeFromUtf8Bytes<List<Person>>(utf8Payload, jsonOptions)
            ?? new List<Person>();
        Console.WriteLine();
        Console.WriteLine($"=== UTF-8 bytes round-trip count: {fromUtf8.Count} ===");

        // --- Section 3: JsonIgnore keeps secrets off the wire ---
        DemonstrateJsonIgnore(jsonOptions);

        // --- Sections 4–5: enum strings + custom DateTime converter ---
        DemonstrateEnumAndCustomConverter(jsonOptions);

        // --- Section 6: JSON versioning — v1 payload without nickname ---
        DemonstrateJsonVersioning(jsonOptions);

        // --- Section 7: cycles — default throws; IgnoreCycles succeeds ---
        DemonstrateReferenceHandler(jsonOptions);

        // --- Section 9: XML AlumniRoster with root + attribute ---
        string xmlPath = Path.Combine(Path.GetTempPath(), "serialization-demo-alumni.xml");
        AlumniRoster rosterDocument = new AlumniRoster
        {
            ExportedOn = DateTime.UtcNow,
            Members = recentGraduates
        };
        SerializationHelpers.WriteXmlToFile(rosterDocument, xmlPath);
        string xmlOnDisk = File.ReadAllText(xmlPath, Encoding.UTF8);

        Console.WriteLine();
        Console.WriteLine("=== XML on disk (excerpt) ===");
        Console.WriteLine(xmlOnDisk.Length > 320 ? xmlOnDisk[..320] + "..." : xmlOnDisk);

        AlumniRoster? fromXml = SerializationHelpers.ReadXmlFromFile<AlumniRoster>(xmlPath);
        Console.WriteLine();
        Console.WriteLine("=== Round-trip from XML ===");
        if (fromXml is not null)
        {
            SerializationHelpers.PrintAlumni(fromXml.Members);
        }

        // --- Section 8c: JsonNode for ad hoc JSON without a fixed type ---
        DemonstrateJsonNode();

        // --- Section 11–12: legacy binary + format choice summary ---
        Console.WriteLine();
        Console.WriteLine("=== BinaryFormatter ===");
        Console.WriteLine("Not executed — obsolete (SYSLIB0011) and unsafe on untrusted input. See Section 11 comments.");

        Console.WriteLine();
        Console.WriteLine("=== Summary ===");
        Console.WriteLine($"JSON file : {jsonPath} ({new FileInfo(jsonPath).Length} bytes)");
        Console.WriteLine($"XML file  : {xmlPath} ({new FileInfo(xmlPath).Length} bytes)");
        Console.WriteLine($"Counts (source / JSON / XML): {recentGraduates.Count} / {fromJson.Count} / {fromXml?.Members.Count ?? 0}");
        Console.WriteLine("Prefer System.Text.Json for new .NET 8 work; XML when a schema or partner requires it.");
    }

    /*
     * =========================================================================
     * SECTION 5 (continued): JsonSerializerOptions — NAMING, CASE, DEPTH
     * =========================================================================
     *
     * Common options used in APIs and config:
     *
     *   Option                      | Purpose
     *   ----------------------------|----------------------------------------
     *   PropertyNamingPolicy        | CamelCase, snake_case (custom), etc.
     *   WriteIndented               | Pretty-print for humans vs compact wire
     *   PropertyNameCaseInsensitive | Match "Name" and "name" on deserialize
     *   DefaultIgnoreCondition      | Skip null / default values on write
     *   MaxDepth                    | Guard against deeply nested attack JSON
     *   ReferenceHandler            | IgnoreCycles or Preserve for graphs
     *   Converters                  | Global custom converters (enums, dates)
     *
     * Pitfall: deserializing untrusted JSON with no MaxDepth — stack overflow
     * or excessive work. ASP.NET sets sensible defaults; console apps should too.
     * -------------------------------------------------------------------------
     */
    private static JsonSerializerOptions BuildTutorialJsonOptions()
    {
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            MaxDepth = 64,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };
        options.Converters.Add(new JsonStringEnumConverter()); // Section 4 — all enums as strings
        return options;
    }

    private static void DemonstrateJsonIgnore(JsonSerializerOptions options)
    {
        MemberProfile profile = new MemberProfile
        {
            Username = "darshan",
            PasswordHash = "sha256:do-not-leak"
        };

        string json = JsonSerializer.Serialize(profile, options);
        Console.WriteLine();
        Console.WriteLine("=== JsonIgnore — password hash omitted ===");
        Console.WriteLine(json);
    }

    private static void DemonstrateEnumAndCustomConverter(JsonSerializerOptions options)
    {
        TieredMember tiered = new TieredMember { Name = "Suyesh", Tier = MembershipTier.FullMember };
        EventRegistration registration = new EventRegistration
        {
            EventName = "Alumni Meetup",
            RegisteredAt = new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc)
        };

        string tierJson = JsonSerializer.Serialize(tiered, options);
        string eventJson = JsonSerializer.Serialize(registration, options);

        Console.WriteLine();
        Console.WriteLine("=== Enum as string + JsonPropertyName on member ===");
        Console.WriteLine(tierJson);

        Console.WriteLine();
        Console.WriteLine("=== Custom JsonConverter — ISO DateTime ===");
        Console.WriteLine(eventJson);
    }

    /*
     * =========================================================================
     * SECTION 6: JSON VERSIONING — MISSING AND EXTRA MEMBERS
     * =========================================================================
     *
     * Evolving DTOs without breaking old clients:
     *
     *   Property type   | Missing in JSON → deserialized value
     *   ----------------|------------------------------------------
     *   string?         | null
     *   int             | 0
     *   bool            | false
     *
     * Extra JSON properties are ignored by default — forward-compatible reads.
     * Prefer adding optional nullable properties instead of renaming in place.
     *
     * Pitfall: renaming a JSON property without [JsonPropertyName] breaks old
     * payloads unless you run a migration or accept both names via custom logic.
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateJsonVersioning(JsonSerializerOptions options)
    {
        string v1Json = """
            {
              "name": "Darshan",
              "institute": "DYP",
              "occupation": "Engineer",
              "yearOfBirth": 2000
            }
            """;

        Person? fromV1 = JsonSerializer.Deserialize<Person>(v1Json, options);

        Console.WriteLine();
        Console.WriteLine("=== v1 JSON (no nickname) ===");
        if (fromV1 is not null)
        {
            Console.WriteLine(fromV1);
            Console.WriteLine($"Nickname: {(fromV1.Nickname is null ? "null (default)" : fromV1.Nickname)}");
        }

        Person v2Person = new Person
        {
            Name = "Suyesh",
            Institute = "KKW",
            Occupation = "Engineer",
            YearOfBirth = 1999,
            Nickname = "Suy"
        };

        string v2Json = JsonSerializer.Serialize(v2Person, options);
        Person? fromV2 = JsonSerializer.Deserialize<Person>(v2Json, options);

        Console.WriteLine();
        Console.WriteLine("=== v2 JSON includes nickname ===");
        Console.WriteLine(v2Json);
        if (fromV2 is not null)
        {
            Console.WriteLine($"Round-trip nickname: {fromV2.Nickname}");
        }

        // Extra property "legacyId" is ignored on read — no error
        string forwardCompatibleJson = """
            {
              "name": "Parth",
              "institute": "DYP",
              "occupation": "Engineer",
              "yearOfBirth": 2002,
              "legacyId": 9001
            }
            """;
        Person? withExtra = JsonSerializer.Deserialize<Person>(forwardCompatibleJson, options);
        Console.WriteLine();
        Console.WriteLine($"=== Extra JSON keys ignored — read name: {withExtra?.Name} ===");
    }

    private static void DemonstrateReferenceHandler(JsonSerializerOptions options)
    {
        OrgUnit root = new OrgUnit { Name = "Engineering" };
        OrgUnit child = new OrgUnit { Name = "Platform" };
        root.Children.Add(child);
        child.Parent = root; // cycle: root → child → root

        JsonSerializerOptions strictOptions = new JsonSerializerOptions(options);
        strictOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;

        string safeJson = JsonSerializer.Serialize(root, strictOptions);
        Console.WriteLine();
        Console.WriteLine("=== ReferenceHandler.IgnoreCycles (parent nulled on second visit) ===");
        Console.WriteLine(safeJson.Length > 180 ? safeJson[..180] + "..." : safeJson);
    }

    /*
     * =========================================================================
     * SECTION 8c: JsonNode — DYNAMIC JSON DOCUMENTS
     * =========================================================================
     *
     * When the schema is unknown or you only need a few fields, parse into
     * JsonNode / JsonObject instead of a static class.
     *
     * JsonNode.Parse returns a mutable tree; use [] indexer and GetValue<T>()
     * for typed reads. Prefer strongly typed DTOs when the shape is stable.
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateJsonNode()
    {
        string payload = """
            {
              "institute": "DYP",
              "count": 3,
              "tags": ["alumni", "2024"]
            }
            """;

        JsonNode? root = JsonNode.Parse(payload);
        string? institute = root?["institute"]?.GetValue<string>();
        int count = root?["count"]?.GetValue<int>() ?? 0;

        Console.WriteLine();
        Console.WriteLine("=== JsonNode — ad hoc field access ===");
        Console.WriteLine($"institute={institute}, count={count}");
    }
}

/*
 * =========================================================================
 * SECTION 10: XmlSerializer — RUNTIME REQUIREMENTS AND FAILURES
 * =========================================================================
 *
 * Common XmlSerializer exceptions and causes:
 *
 *   Exception / symptom              | Typical cause
 *   ---------------------------------|----------------------------------
 *   InvalidOperationException        | No public parameterless constructor
 *   InvalidOperationException        | Property type not supported (e.g. Dictionary without adapter)
 *   FileNotFoundException at ctor    | XmlSerializer temp assembly (historical — rare on .NET Core)
 *   Wrong file on deserialize        | Legacy sample wrote .xml but read .txt — always match paths
 *
 * Newtonsoft.Json (Json.NET) — still seen in older codebases:
 *
 *   JsonConvert.SerializeObject(obj);
 *   JsonConvert.DeserializeObject<T>(json);
 *
 * Same object ↔ JSON idea as System.Text.Json; prefer System.Text.Json for
 * new .NET 8 work unless a dependency requires Newtonsoft.
 * -------------------------------------------------------------------------
 */

/*
 * =========================================================================
 * SECTION 11: BinaryFormatter — LEGACY (DO NOT USE IN NEW CODE)
 * =========================================================================
 *
 * BinaryFormatter serialized .NET object graphs as opaque binary streams.
 * It required [Serializable] on types and felt convenient — but deserializing
 * untrusted binary can execute arbitrary code (gadget chain attacks).
 *
 * .NET 5+ marks BinaryFormatter obsolete (SYSLIB0011). .NET 8 disables it
 * unless a compatibility switch is set — do not enable that for new apps.
 *
 * Legacy pattern (recognition only — NOT compiled here):
 *
 *   using System.Runtime.Serialization.Formatters.Binary;
 *
 *   FileStream stream = new FileStream(path, FileMode.Create);
 *   BinaryFormatter formatter = new BinaryFormatter();  // SYSLIB0011
 *   formatter.Serialize(stream, list);
 *
 *   List<Person> copy = (List<Person>)formatter.Deserialize(stream);
 *
 * Modern replacements:
 *
 *   Use case              | Replacement
 *   ----------------------|------------------------------------------
 *   REST / web APIs       | System.Text.Json
 *   Config / integration  | JSON or XML
 *   Cross-language binary | Protocol Buffers, MessagePack (NuGet)
 *   .NET-only performance | System.Text.Json UTF-8 bytes
 *
 * Treat existing .bin files as untrusted until migrated to JSON/XML.
 * -------------------------------------------------------------------------
 */

/*
 * =============================================================================
 * QUICK REFERENCE
 * =============================================================================
 *
 * --- Vocabulary ---
 *
 *   Serialize       object graph → bytes or text
 *   Deserialize     wire format → new object instances
 *   Wire format     JSON, XML, protobuf, etc.
 *
 * --- System.Text.Json ---
 *
 *   JsonSerializer.Serialize(obj, options)
 *   JsonSerializer.Deserialize<T>(json, options)
 *   SerializeToUtf8Bytes / Deserialize from ReadOnlySpan<byte>
 *   JsonSerializerOptions — CamelCase, WriteIndented, CaseInsensitive, MaxDepth
 *   ReferenceHandler.IgnoreCycles | Preserve
 *
 * --- JSON attributes ---
 *
 *   [JsonPropertyName("wire")]     map name
 *   [JsonIgnore]                   omit property
 *   [JsonInclude]                  include non-public member (opt-in)
 *   [JsonPropertyOrder(n)]         write order
 *   [JsonConverter(typeof(T))]     custom mapping
 *   JsonStringEnumConverter          enums as strings
 *
 * --- Versioning ---
 *
 *   Missing property → type default; extra keys → ignored
 *   Add nullable optional fields; avoid silent renames
 *
 * --- XmlSerializer ---
 *
 *   new XmlSerializer(typeof(T)); Serialize/Deserialize(stream)
 *   [XmlRoot], [XmlElement], [XmlAttribute], [XmlIgnore]
 *   Requires public parameterless ctor + public properties
 *
 * --- [Serializable] ---
 *
 *   Legacy binary only — ignored by System.Text.Json and XmlSerializer
 *
 * --- JsonNode ---
 *
 *   JsonNode.Parse(json); node["key"]?.GetValue<T>()
 *
 * --- BinaryFormatter ---
 *
 *   Obsolete, unsafe — use JSON, XML, or protobuf/MessagePack
 *
 * --- Format choice ---
 *
 *   ASP.NET Core APIs     → System.Text.Json
 *   Partner XSD / SOAP    → XML
 *   High-through perf     → UTF-8 JSON or external binary serializer
 *
 * =============================================================================
 */
