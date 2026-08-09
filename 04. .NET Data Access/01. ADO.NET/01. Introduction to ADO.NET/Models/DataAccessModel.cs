namespace IntroductionToAdoNet.Models;

/*
 * FILE ROLE: Enum for connected vs disconnected data-access models (Section 2).
 * SECTIONS IN THIS FILE:
 *   2. Connected vs disconnected model — DataAccessModel enum
 */

/*
 * =========================================================================
 * SECTION 2: CONNECTED VS DISCONNECTED MODEL
 * =========================================================================
 *
 * ADO.NET supports two complementary ways to work with data:
 *
 *   CONNECTED MODEL                         DISCONNECTED MODEL
 *   ----------------                        ------------------
 *   Connection open for the read/write      Data copied into DataSet / DataTable
 *   DbConnection → DbCommand →            Connection closed; rows live in memory
 *   DbDataReader (forward-only stream)    DataAdapter fills/updates the cache
 *
 *   Best for:                               Best for:
 *   • Large result sets (stream rows)       • Offline editing / WinForms grids
 *   • Low memory — one row at a time        • Passing data between tiers (DataSet)
 *   • Stored procedures, reporting          • Snapshot caches, XML serialization
 *   • Most web/API read paths               • Batch merge scenarios (Fill + Update)
 *
 *   Key types (preview — detail in later chapters):
 *     Connected     → ch.02 SqlConnection, ch.03 SqlCommand, ch.04 SqlDataReader
 *     Disconnected  → ch.05 DataSet, DataTable, SqlDataAdapter
 *
 * DataTableReader (used in Services/ReaderDemo.cs) bridges both worlds: it is a
 * DbDataReader over an in-memory DataTable — useful to learn reader semantics
 * without a database.
 * -------------------------------------------------------------------------
 */
public enum DataAccessModel
{
    Connected,
    Disconnected,
}
