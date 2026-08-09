# Product Export Scanner — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| Custom attributes defined with correct AttributeUsage | 15 |
| `BuildManifest` reads EntityTable + Exportable + DisplayLabel | 25 |
| Stock omitted (no Exportable) | 10 |
| `ExportRow` column order matches manifest | 20 |
| Decimal formatted F2 | 10 |
| Null instance guard | 5 |
| `ExportAll` preserves item order | 10 |
| Demo prints table + headers + rows | 5 |

## AI Review Prompt

Evaluate ProductExportScanner against PROBLEM.md. Score /100. Verify reflection attribute reading, export column filtering, and value formatting. Strengths, bugs, verdict.

---

## Model Answer Checklist

- [ ] Manifest has 3 columns: Sku, Name, Price (not Stock)
- [ ] Headers: SKU Code, Product Name, Unit Price
- [ ] TableName is Products
- [ ] Price 12.5 exports as 12.50
