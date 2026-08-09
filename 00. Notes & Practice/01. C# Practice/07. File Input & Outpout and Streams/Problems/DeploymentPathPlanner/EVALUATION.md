# Deployment Path Planner — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| ApplicationData + LocalApplicationData roots | 30 |
| BaseDirectory + GetFullPath for assets | 20 |
| Machine log path segments | 15 |
| Temp export GUID uniqueness | 15 |
| ChangeExtension correct usage | 10 |
| Path.Combine throughout | 10 |

## AI Review Prompt

Evaluate DeploymentPathPlanner against PROBLEM.md. Score /100. No hard-coded drive paths, no Console in planner. Verdict Pass/Revise.

---

## Model Answer Checklist

- [ ] Roaming path under user ApplicationData
- [ ] ResolveBundledAsset is absolute after GetFullPath
- [ ] Temp path includes .csv extension
