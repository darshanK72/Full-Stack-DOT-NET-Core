/*
 * PROBLEM: Config Snapshot Merger
 *
 * DevOps merges environment JSON settings with hotfix patches using JsonNode
 * so config shapes can evolve without a fixed C# model for every key.
 *
 * This exercise covers:
 *   ch01 — JsonNode.Parse and mutable JSON document trees
 *   ch01 — reading/writing JSON without fixed types
 *   ch01 — JsonSerializerOptions for indented output
 */

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ApplicationConfig
{
    /*
     * Shallow JSON merge and navigation helpers.
     * No Console I/O in this class.
     */
    class ConfigSnapshotMerger
    {
        /*
         * Parses base and patch as JsonObject; copies each patch top-level
         * property onto base (replace existing keys). Returns indented JSON string.
         *
         * Nested objects from patch replace entire base nested object (shallow merge).
         */
        public string MergeJson(string baseJson, string patchJson)
        {
            // TODO: JsonNode.Parse; iterate patch properties; ToJsonString indented
            throw new NotImplementedException();
        }

        /*
         * Reads integer at top-level key when present and convertible.
         *
         * Returns false and value 0 when root is null or key missing/not int.
         */
        public bool TryGetInt(JsonNode? root, string key, out int value)
        {
            value = 0;
            // TODO: navigate root[key]; try get int
            throw new NotImplementedException();
        }

        /*
         * Returns top-level property names sorted alphabetically.
         *
         * Empty list when json is not a JSON object.
         */
        public IReadOnlyList<string> ListTopLevelKeys(string json)
        {
            // TODO: parse; collect keys; sort
            throw new NotImplementedException();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // TODO: define base and patch JSON strings per PROBLEM.md
            // TODO: merge and print indented result
            // TODO: TryGetInt on timeoutSeconds and missing key
            // TODO: ListTopLevelKeys on merged json
            throw new NotImplementedException();
        }
    }
}
