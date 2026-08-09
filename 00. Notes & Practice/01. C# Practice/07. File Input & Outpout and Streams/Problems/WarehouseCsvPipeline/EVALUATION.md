# Warehouse CSV Pipeline — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| Quote-aware SplitQuotedLine | 25 |
| EscapeField / BuildRow export | 20 |
| Import partial success + line errors | 25 |
| InvariantCulture parse/format | 15 |
| TextReader file + StringReader demo | 10 |
| Header skip + comment/blank skip | 5 |

## AI Review Prompt

Evaluate StockCsvPipeline against PROBLEM.md. Score /100. Quoted comma row imports correctly, bad row does not abort entire file. Verdict Pass/Revise.

---

## Model Answer Checklist

- [ ] Acme comma name imports as single name field
- [ ] Mixed file returns ≥1 item and ≥1 error string with line number
- [ ] Export round-trip readable first data line
