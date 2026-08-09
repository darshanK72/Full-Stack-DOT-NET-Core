# Hotel Reservation Notifier — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| ReservationService orchestration | 25 |
| StubRoomAvailability behavior | 15 |
| FakeGuestNotifier message capture | 15 |
| Three manual-double tests | 30 |
| Failure path skips notification | 10 |
| Demo Main | 5 |

## AI Review Prompt

Evaluate HotelReservationNotifier against PROBLEM.md. Confirm stub vs fake roles, no Moq, constructor injection. Score /100, verdict.

---

## Model Answer Checklist

- [ ] Unavailable room → Success false, zero notifications
- [ ] Available room → exactly one fake message
- [ ] Tests do not use real network
