---
module: 09. Unit Testing
difficulty: Hard
chapters: 04 Mocking & Test Doubles
domain: HospitalityBooking
---

# Hotel Reservation Notifier

Build a **.NET 8 solution** where `ReservationService` depends on injectable interfaces. Write **manual test doubles** (stub + fake) and xUnit tests without Moq.

## Business context

A hotel booking API confirms room availability before charging a card and sends guest notifications. Unit tests must not call a real PMS or SMTP server — use hand-written doubles.

## Solution layout

```
HotelReservationNotifier/
  Program.cs                         ← models, interfaces, ReservationService
  HotelReservationNotifier.Tests/
    TestDoubles/
      StubRoomAvailability.cs        ← implement TODO
      FakeGuestNotifier.cs           ← implement TODO
    ReservationManualDoubleTests.cs  ← complete test TODOs
```

## Definitions (production)

**Record `BookingRequest`:** `ConfirmationCode` (string), `GuestEmail`, `RoomType`, `Nights` (int ≥ 1)

**Record `BookingResult`:** `Success` (bool), `ConfirmationCode`, `Message`

**Interface `IRoomAvailability`:** `bool IsAvailable(string roomType, int nights)`

**Interface `IGuestNotifier`:** `void SendConfirmation(string guestEmail, string confirmationCode)`

**Class `ReservationService`**

- Constructor `(IRoomAvailability availability, IGuestNotifier notifier)`
- `BookingResult ConfirmBooking(BookingRequest request)` — if not available, return failure without notifying; if available, send confirmation and return success with same confirmation code

## Manual doubles (implement TODOs)

**`StubRoomAvailability`** — constructor takes `bool available`; `IsAvailable` returns that flag regardless of args

**`FakeGuestNotifier`** — in-memory `List<string> SentMessages`; `SendConfirmation` appends `"email|code"` string

## Tests

| Test | Assert |
|------|--------|
| `ConfirmBooking_WhenRoomAvailable_ReturnsSuccess` | Success true |
| `ConfirmBooking_WhenRoomAvailable_SendsExactlyOneNotification` | Fake list count 1 |
| `ConfirmBooking_WhenRoomUnavailable_DoesNotNotify` | Fake list empty |

## Demo Main

Run success path with stub=true and fake; print sent messages.

## Constraints

- No Moq in this project — manual doubles only
- Constructor injection on ReservationService

## Non-goals

Moq, database, HTTP

## Evaluation

[EVALUATION.md](EVALUATION.md)
