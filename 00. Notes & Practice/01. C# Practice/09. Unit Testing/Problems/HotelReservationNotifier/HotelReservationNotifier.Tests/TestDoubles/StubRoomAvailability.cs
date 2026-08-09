using System;
using System.Collections.Generic;
using HospitalityBooking;

namespace HotelReservationNotifier.Tests.TestDoubles;

/*
 * Stub — returns fixed availability flag regardless of room type or nights.
 */
public sealed class StubRoomAvailability : IRoomAvailability
{
    private readonly bool _available;

    public StubRoomAvailability(bool available)
    {
        _available = available;
    }

    public bool IsAvailable(string roomType, int nights)
    {
        // TODO: return _available
        throw new NotImplementedException();
    }
}

/*
 * Fake — working in-memory notifier that records sent messages.
 */
public sealed class FakeGuestNotifier : IGuestNotifier
{
    public List<string> SentMessages { get; } = new List<string>();

    public void SendConfirmation(string guestEmail, string confirmationCode)
    {
        // TODO: append "email|code" to SentMessages
        throw new NotImplementedException();
    }
}
