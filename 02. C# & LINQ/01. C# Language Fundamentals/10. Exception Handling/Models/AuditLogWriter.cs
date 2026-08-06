using System;

namespace ExceptionHandling.Models;

/*
 * Minimal IDisposable type for the using-statement preview in Program.cs.
 * Full disposal patterns are covered in the OOP / resource-management chapters.
 */
public class AuditLogWriter : IDisposable
{
    private bool _disposed;

    public string LastEntry { get; private set; } = string.Empty;

    public void WriteEntry(string entry)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(AuditLogWriter));
        }

        LastEntry = entry;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            LastEntry = LastEntry + " [closed]";
            _disposed = true;
        }
    }
}
