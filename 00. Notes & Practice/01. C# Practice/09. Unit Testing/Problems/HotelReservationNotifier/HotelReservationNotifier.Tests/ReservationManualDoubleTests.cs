using System;
using HospitalityBooking;
using HotelReservationNotifier.Tests.TestDoubles;
using Xunit;

namespace HotelReservationNotifier.Tests;

public sealed class ReservationManualDoubleTests
{
    private static BookingRequest SampleRequest() =>
        new BookingRequest("HB-9001", "guest@example.com", "Deluxe", 2);

    [Fact]
    public void ConfirmBooking_WhenRoomAvailable_ReturnsSuccess()
    {
        // TODO: stub true, fake notifier, Assert.True result.Success
        throw new NotImplementedException();
    }

    [Fact]
    public void ConfirmBooking_WhenRoomAvailable_SendsExactlyOneNotification()
    {
        // TODO: assert fake.SentMessages.Count == 1
        throw new NotImplementedException();
    }

    [Fact]
    public void ConfirmBooking_WhenRoomUnavailable_DoesNotNotify()
    {
        // TODO: stub false; assert SentMessages empty and Success false
        throw new NotImplementedException();
    }
}
