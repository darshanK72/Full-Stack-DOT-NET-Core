namespace IntroductionToAdoNet.Models;

/*
 * FILE ROLE: Enum for DbCommand execute method families (Section 4 preview).
 * SECTIONS IN THIS FILE:
 *   4. DbCommand — send SQL or stored procedures (preview) — CommandExecuteMethod enum
 */

public enum CommandExecuteMethod
{
    NonQuery,
    Scalar,
    Reader,
}
