# Clinic Appointment Manager — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| Add doctor/appointment rules | 15 |
| GetCompletedCount | 10 |
| GetAverageDurationByType (zeros included) | 25 |
| GetDoctorWorkloadRanking + tie-break | 30 |
| Structure & clarity | 10 |
| Constraints | 10 |

## AI Review Prompt

Evaluate ClinicAppointmentManager against PROBLEM.md. Score /100, per-method feedback, strengths, bugs, improvements, verdict.

---

## Model Answer Checklist

- [ ] Unknown doctor cannot receive appointment
- [ ] Only COMPLETED in averages and ranking
- [ ] All enum types present in average map with 0 default
- [ ] Ranking tie-break lower DoctorId
- [ ] topK edge cases
