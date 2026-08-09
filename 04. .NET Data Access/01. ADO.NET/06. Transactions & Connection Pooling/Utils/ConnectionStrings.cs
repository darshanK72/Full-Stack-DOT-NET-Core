using System;

namespace TransactionsAndConnectionPooling.Utils;

/*
 * FILE ROLE: Connection string constants for LocalDB master and AdoNetTutorial, including pool keywords.
 *
 * SECTIONS IN THIS FILE:
 *   1. Connection strings — AdoNetTutorial on LocalDB
 */

/*
 * SECTION 1: CONNECTION STRINGS - AdoNetTutorial on LocalDB
 *
 * Chapters 02-03 introduced SqlConnection and SqlCommand. This chapter adds
 * transaction keywords to the same connection string family.
 *
 * Pool-related keywords (same for every connection that shares identical text):
 *   Min Pool Size=0   - pool starts empty; grows on demand
 *   Max Pool Size=100 - cap on pooled connections per unique string
 *
 * Pooling is ON by default for SqlConnection. Set "Pooling=false" only when
 * debugging connection leaks or testing cold-open latency.
 *
 * Master connection - used once at startup to CREATE DATABASE if missing.
 */
internal static class ConnectionStrings
{
    public const string Master =
        "Server=(localdb)\\MSSQLLocalDB;Database=master;Integrated Security=true;TrustServerCertificate=true;";

    // Identical pool key for every open in this demo - connections reuse the same pool
    public const string AdoNetTutorial =
        "Server=(localdb)\\MSSQLLocalDB;Database=AdoNetTutorial;Integrated Security=true;TrustServerCertificate=true;Min Pool Size=0;Max Pool Size=100;";
}
