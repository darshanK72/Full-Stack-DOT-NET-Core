/*
 * PROBLEM: Hotel Reservation Notifier
 *
 * A hotel booking API confirms availability before notifying guests. Manual test
 * doubles replace PMS and SMTP dependencies in fast unit tests.
 *
 * This exercise covers:
 *   ch04 — stub returns canned answers (StubRoomAvailability)
 *   ch04 — fake records side effects (FakeGuestNotifier)
 *   ch04 — constructor injection and interface seams
 *   ch04 — testing behavior without real infrastructure
 */

using System;

namespace HospitalityBooking
{
    public sealed record BookingRequest(
        string ConfirmationCode,
        string GuestEmail,
        string RoomType,
        int Nights);

    public sealed record BookingResult(
        bool Success,
        string ConfirmationCode,
        string Message);

    /*
     * External room inventory seam — production would call PMS API.
     */
    public interface IRoomAvailability
    {
        bool IsAvailable(string roomType, int nights);
    }

    /*
     * Guest notification seam — production would send SMTP email.
     */
    public interface IGuestNotifier
    {
        void SendConfirmation(string guestEmail, string confirmationCode);
    }

    /*
     * Orchestrates availability check then notification.
     * Dependencies injected via constructor — no new() inside methods.
     */
    public sealed class ReservationService
    {
        private readonly IRoomAvailability _availability;
        private readonly IGuestNotifier _notifier;

        public ReservationService(IRoomAvailability availability, IGuestNotifier notifier)
        {
            _availability = availability;
            _notifier = notifier;
        }

        /*
         * When room unavailable: Success=false, Message explains, no notification.
         * When available: send confirmation, Success=true.
         */
        public BookingResult ConfirmBooking(BookingRequest request)
        {
            // TODO: check availability; notify on success; return BookingResult
            throw new NotImplementedException();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // TODO: stub available=true, fake notifier; confirm booking; print SentMessages
            throw new NotImplementedException();
        }
    }
}
