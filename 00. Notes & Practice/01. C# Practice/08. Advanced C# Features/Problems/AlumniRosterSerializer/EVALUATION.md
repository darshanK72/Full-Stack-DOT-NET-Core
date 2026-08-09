# Alumni Roster Serializer — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| `MembershipTier` string enum on wire | 10 |
| `[JsonPropertyName]` / `[XmlElement]` nickname mapping | 15 |
| `[JsonIgnore]` excludes `InternalNotes` from JSON | 15 |
| `ToJson` / `FromJson` round trip | 20 |
| `ToXml` / `FromXml` round trip | 15 |
| Missing optional `nickname` deserializes safely | 10 |
| `JsonRoundTripPreservesTier` | 10 |
| CamelCase + enum converter in options | 5 |

## AI Review Prompt

Evaluate AlumniRosterSerializer against PROBLEM.md. Score /100. Verify JSON/XML attributes, enum string wire format, ignored sensitive field, and versioning tolerance for missing nickname. Strengths, bugs, verdict.

---

## Model Answer Checklist

- [ ] JSON output contains `"tier":"Alumni"` not numeric enum
- [ ] JSON never contains `internalNotes`
- [ ] XML contains `<Nickname>` when set
- [ ] Legacy JSON without nickname yields null `Nickname`
- [ ] `CreateApiOptions` registers `JsonStringEnumConverter`
