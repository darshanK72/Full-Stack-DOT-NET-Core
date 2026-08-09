---
module: 08. Advanced C# Features
difficulty: Medium
chapters: 01 Serialization & Desiralization
domain: EducationRecords
---

# Alumni Roster Serializer

Build a **.NET 8 console application from scratch** that serializes and deserializes alumni records to JSON and XML using `System.Text.Json` and `XmlSerializer`, honoring wire-format attributes and serializer options.

## Business context

An alumni association exports member rosters to partner institutes. JSON feeds a public API; XML feeds a legacy mainframe. Sensitive internal notes must never appear on the wire, and membership tier enums must serialize as readable strings.

## Definitions

**Enum `MembershipTier`**

- Values: `Active`, `Alumni`, `Honorary`
- Apply `[JsonConverter(typeof(JsonStringEnumConverter))]` on the enum

**Class `AlumniMember`**

- `Name` (string)
- `Institute` (string)
- `YearOfBirth` (int)
- `[JsonPropertyName("nickname")]` / `[XmlElement("Nickname")]` on `Nickname` (string?, optional)
- `Tier` (`MembershipTier`)
- `[JsonIgnore]` on `InternalNotes` (string) — never written to JSON

**Class `AlumniRosterSerializer`**

- `string ToJson(AlumniMember member, bool indented = false)` — `JsonSerializer.Serialize` with options: camelCase naming when `indented` is false (use `PropertyNamingPolicy = JsonNamingPolicy.CamelCase`), `WriteIndented = indented`
- `AlumniMember FromJson(string json)` — deserialize; missing optional `nickname` must not throw
- `string ToXml(AlumniMember member)` — `XmlSerializer` to string via `StringWriter`
- `AlumniMember FromXml(string xml)` — deserialize from string
- `bool JsonRoundTripPreservesTier(AlumniMember original)` — serialize then deserialize; compare `Tier` equality

**Static class `RosterJsonOptions`**

- `static JsonSerializerOptions CreateApiOptions()` — returns shared options: camelCase, indented, `JsonStringEnumConverter` registered

## Demo Main

1. Create member with nickname, tier `Alumni`, and internal notes.
2. Print JSON (compact) — verify `InternalNotes` absent and tier is string.
3. Round-trip JSON and print `Name` + `Tier`.
4. Serialize same member to XML; print snippet showing `Nickname` element.
5. Deserialize legacy JSON missing `nickname`; print `Nickname` is null.

## Constraints

- net8, explicit usings, no third-party serializers
- `decimal` not required; use `int` for birth year
- Do not use `BinaryFormatter`

## Non-goals

Database persistence, circular graphs, custom `JsonConverter` for dates

## Evaluation

[EVALUATION.md](EVALUATION.md)
