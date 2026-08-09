# Legacy Inventory Migrator — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| Legacy ArrayList with mixed types + boxing demo | 15 |
| Hashtable sample + lookup | 15 |
| Import maps Product → List&lt;Product&gt; | 20 |
| Import maps string/int pairs → Dictionary | 20 |
| Wrong-type legacy entries skipped safely | 15 |
| CatalogTotal typed foreach, no cast | 10 |
| Demo explains cast removal | 5 |

## AI Review Prompt

Evaluate LegacyInventoryMigrator against PROBLEM.md. Score /100. Verify boxing/unboxing handled, generic collections used for modern side, no InvalidCastException on happy path. Verdict Pass/Revise.

---

## Model Answer Checklist

- [ ] UnboxFirstCount uses explicit (int) cast
- [ ] Modern side uses List and Dictionary only
- [ ] Import does not add duplicate SKU keys silently (last wins or skip — document choice)
