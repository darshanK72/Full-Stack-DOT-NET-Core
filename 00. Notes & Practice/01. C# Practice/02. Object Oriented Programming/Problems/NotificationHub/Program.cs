/*
 * PROBLEM: Notification Hub
 *
 * Operations sends email and SMS alerts. Subscribers log deliveries for audit.
 * Design mirrors publisher/subscriber with typed events and pluggable senders.
 *
 * This exercise covers:
 *   ch06 — INotificationSender interface; interface-based design
 *   ch08 — event keyword; EventHandler<T>; subscribe/unsubscribe
 *   ch09 — notification sender real-world example
 */

using System;
using System.Collections.Generic;

namespace Alerting
{
    enum AlertSeverity
    {
        Info,
        Warning,
        Critical
    }

    interface INotificationSender
    {
        string ChannelName { get; }
        bool Send(string recipient, string message, AlertSeverity severity);
    }

    class EmailSender : INotificationSender
    {
        public string ChannelName => "Email";

        public bool Send(string recipient, string message, AlertSeverity severity)
        {
            // TODO: validate recipient contains '@'; return false if invalid
            throw new NotImplementedException();
        }
    }

    class SmsSender : INotificationSender
    {
        public string ChannelName => "Sms";

        public bool Send(string recipient, string message, AlertSeverity severity)
        {
            // TODO: validate 10-15 digits only after trim; return false if invalid
            throw new NotImplementedException();
        }
    }

    class AlertPublishedEventArgs : EventArgs
    {
        public string Recipient { get; }
        public string Message { get; }
        public AlertSeverity Severity { get; }
        public string ChannelName { get; }
        public bool Success { get; }

        public AlertPublishedEventArgs(
            string recipient,
            string message,
            AlertSeverity severity,
            string channelName,
            bool success)
        {
            Recipient = recipient;
            Message = message;
            Severity = severity;
            ChannelName = channelName;
            Success = success;
        }
    }

    class NotificationHub
    {
        private readonly List<INotificationSender> _senders;

        public event EventHandler<AlertPublishedEventArgs>? AlertPublished;

        public NotificationHub(IEnumerable<INotificationSender> senders)
        {
            _senders = new List<INotificationSender>(senders);
        }

        public bool Publish(string recipient, string message, AlertSeverity severity, string channelName)
        {
            // TODO: find sender by case-insensitive ChannelName; false if none
            // TODO: call Send; raise AlertPublished with outcome (even on failure)
            throw new NotImplementedException();
        }
    }

    class AuditLog
    {
        private readonly NotificationHub _hub;
        private readonly List<string> _entries = new List<string>();

        public IReadOnlyList<string> Entries => _entries.AsReadOnly();

        public AuditLog(NotificationHub hub)
        {
            _hub = hub;
            // TODO: subscribe to AlertPublished
            throw new NotImplementedException();
        }

        public void Unsubscribe()
        {
            // TODO: remove handler from hub
            throw new NotImplementedException();
        }

        private void OnAlertPublished(object? sender, AlertPublishedEventArgs e)
        {
            // TODO: append "[{Severity}] {ChannelName} -> {Recipient}: {Success}"
            throw new NotImplementedException();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // TODO: wire hub with both senders; subscribe audit log
            // TODO: publish valid/invalid recipients on both channels; print entries
            // TODO: unsubscribe; publish again — no new entries
            throw new NotImplementedException();
        }
    }
}
