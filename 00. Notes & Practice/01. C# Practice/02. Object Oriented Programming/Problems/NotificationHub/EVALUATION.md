# Notification Hub — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| INotificationSender implementations | 20 |
| Publish routing + validation | 20 |
| AlertPublished event + EventArgs | 25 |
| AuditLog subscribe/unsubscribe | 20 |
| Event not publicly invokable | 10 |
| Demo covers failure paths | 5 |

## AI Review Prompt

Evaluate NotificationHub against PROBLEM.md. Score /100, event/delegate safety, interface usage, strengths, verdict.

---

## Model Answer Checklist

- [ ] Unknown channel returns false
- [ ] Event raised on failed send
- [ ] AuditLog entries formatted correctly
- [ ] After Unsubscribe, no new entries
