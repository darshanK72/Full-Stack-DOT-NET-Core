# Article Tag Manager — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| Article tags case-insensitive unique | 20 |
| Union / Intersect / Except analytics | 25 |
| UnionWith merge in place | 10 |
| IEqualityComparer Subscriber dedup | 20 |
| Default HashSet reference equality contrast | 10 |
| Duplicate Add returns false | 10 |
| Demo output clear | 5 |

## AI Review Prompt

Evaluate ArticleTagManager against PROBLEM.md. Score /100. Verify comparer hash/equals contract for email, set ops correct. Verdict Pass/Revise.

---

## Model Answer Checklist

- [ ] AddTag("linq") when "LINQ" exists → false
- [ ] Union includes tags from both articles
- [ ] Subscriber comparer set Count == 1 for same email
