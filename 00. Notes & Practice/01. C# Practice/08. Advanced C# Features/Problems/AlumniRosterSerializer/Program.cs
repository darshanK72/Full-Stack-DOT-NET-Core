/*
 * PROBLEM: Alumni Roster Serializer
 *
 * An alumni association exports member records to JSON for a public API and to
 * XML for a legacy mainframe. Wire-format attributes control naming and omit
 * sensitive internal notes from JSON payloads.
 *
 * This exercise covers:
 *   ch01 — System.Text.Json Serialize/Deserialize
 *   ch01 — JsonPropertyName, JsonIgnore, JsonStringEnumConverter
 *   ch01 — XmlSerializer with XmlElement
 *   ch01 — missing optional members on deserialize (versioning)
 */

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace EducationRecords
{
    /*
     * Membership tier written as string names on the JSON wire (not 0, 1, 2).
     */
    [JsonConverter(typeof(JsonStringEnumConverter))]
    enum MembershipTier
    {
        Active,
        Alumni,
        Honorary
    }

    /*
     * Alumni member row — attributes map C# names to JSON/XML wire names.
     * InternalNotes must never appear in JSON output.
     */
    class AlumniMember
    {
        public string Name { get; set; } = string.Empty;
        public string Institute { get; set; } = string.Empty;
        public int YearOfBirth { get; set; }

        [JsonPropertyName("nickname")]
        [XmlElement("Nickname")]
        public string? Nickname { get; set; }

        public MembershipTier Tier { get; set; }

        [JsonIgnore]
        public string InternalNotes { get; set; } = string.Empty;
    }

    /*
     * Shared JsonSerializerOptions for API-style output.
     * No Console I/O in this class.
     */
    static class RosterJsonOptions
    {
        /*
         * Returns options with camelCase naming, indentation, and string enums.
         */
        public static JsonSerializerOptions CreateApiOptions()
        {
            // TODO: build and return JsonSerializerOptions with camelCase, WriteIndented, enum converter
            throw new NotImplementedException();
        }
    }

    /*
     * JSON and XML round-trip helpers for AlumniMember.
     * No Console I/O in this class.
     */
    class AlumniRosterSerializer
    {
        /*
         * Serializes member to JSON.
         *
         * When indented is false: camelCase, compact.
         * When indented is true: camelCase, pretty-printed.
         */
        public string ToJson(AlumniMember member, bool indented = false)
        {
            // TODO: JsonSerializer.Serialize with appropriate options
            throw new NotImplementedException();
        }

        /*
         * Deserializes JSON to AlumniMember.
         *
         * Missing optional nickname must deserialize with Nickname = null.
         */
        public AlumniMember FromJson(string json)
        {
            // TODO: JsonSerializer.Deserialize
            throw new NotImplementedException();
        }

        /*
         * Serializes member to XML string via XmlSerializer and StringWriter.
         */
        public string ToXml(AlumniMember member)
        {
            // TODO: XmlSerializer serialize to string
            throw new NotImplementedException();
        }

        /*
         * Deserializes XML string to AlumniMember.
         */
        public AlumniMember FromXml(string xml)
        {
            // TODO: XmlSerializer deserialize from string
            throw new NotImplementedException();
        }

        /*
         * Returns true when Tier survives a JSON serialize/deserialize cycle.
         */
        public bool JsonRoundTripPreservesTier(AlumniMember original)
        {
            // TODO: round-trip and compare Tier
            throw new NotImplementedException();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // TODO: create member with nickname, tier Alumni, internal notes
            // TODO: print compact JSON — verify no internalNotes, tier is string
            // TODO: JSON round-trip and print Name + Tier
            // TODO: serialize to XML and print Nickname element snippet
            // TODO: deserialize legacy JSON without nickname; print Nickname is null
            throw new NotImplementedException();
        }
    }
}
