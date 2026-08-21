using IntroductionToAdoNet.Models;

namespace IntroductionToAdoNet.Utils;

/*
 * FILE ROLE: Describes when to use connected vs disconnected models (Section 2).
 * SECTIONS IN THIS FILE:
 *   2. Connected vs disconnected model — ModelComparison helper
 */

public static class ModelComparison
{
    public static string Describe(DataAccessModel model)
    {
        return model switch
        {
            DataAccessModel.Connected =>
                "Stream rows while DbConnection is open; dispose reader and connection promptly.",
            DataAccessModel.Disconnected =>
                "Fill DataTable/DataSet, close connection, manipulate rows in memory.",
            _ => string.Empty,
        };
    }
}
