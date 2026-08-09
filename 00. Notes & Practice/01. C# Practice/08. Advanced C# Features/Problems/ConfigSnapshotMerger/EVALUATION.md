# Config Snapshot Merger — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| JsonNode.Parse both documents | 10 |
| Shallow top-level merge | 25 |
| Indented ToJsonString output | 10 |
| TryGetInt success path | 15 |
| TryGetInt false on missing/null | 10 |
| ListTopLevelKeys sorted | 15 |
| Demo nested features replacement | 10 |
| Null root guard | 5 |

## AI Review Prompt

Evaluate ConfigSnapshotMerger against PROBLEM.md. Score /100. Verify JsonNode merge semantics (shallow), TryGetInt, and key listing. Strengths, bugs, verdict.

---

## Model Answer Checklist

- [ ] timeoutSeconds becomes 45 after merge
- [ ] features object matches patch entirely (analytics present)
- [ ] appName unchanged from base
- [ ] Top-level keys include appName, features, timeoutSeconds
