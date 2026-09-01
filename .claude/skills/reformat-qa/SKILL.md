---
name: reformat-qa
description: Rewrites an INTERVIEW_QA.md file into the two-section format — Concepts (one-line bullets) and Answer (flowing paragraph)
argument-hint: [path/to/INTERVIEW_QA.md]
allowed-tools: [Read, Edit, Write, Glob]
---

# Reformat Q&A

Rewrite every question in the target file into the two-section format described below.

## Target File

$ARGUMENTS

If no argument is provided, ask the user which file to reformat.

## Output Format

Each question must follow this exact structure:

```
---

## Q{N}. {Question text}

**Concepts**
- {one-line concept, no explanation}
- {one-line concept, no explanation}
- ...

**Answer**

{Flowing paragraph(s). First person. Causally connected sentences. No bullet points unless the content is naturally enumerable steps or parallel options.}

```

## Rules for Concepts

- Each bullet is a noun phrase naming one technical concept, mechanism, or trade-off the answer relies on.
- One line only — no colon explanations, no sub-bullets.
- 3 to 7 bullets per question. Cut anything generic or obvious.
- Examples of the right tone: "Keyset pagination vs offset pagination", "IDisposable cleanup on HttpResponseMessage", "ValidateOnBuild — fail-fast at startup"

## Rules for Answer

**Voice and style**
- Write in first person: "I would...", "The key is...", "The reason this matters is..."
- Sentences connect causally: use "since", "because", "which means", "so", "rather than"
- Dense but readable — a person can read it once and understand it completely
- Match the depth to the question: a concept question gets prose; a code-review question walks through each problem in order

**What to include**
- Start with the core insight or mechanism, not a definition
- Explain why the behaviour exists, not just what it is
- Where multiple options or problems exist, address them in logical order within the paragraph
- Include a code block only when the answer genuinely requires showing syntax that words alone cannot convey

**What to exclude**
- No meta-commentary: no "it is worth noting", "it is important to remember", "as a developer", "in practice you should"
- No cross-references: no "as mentioned earlier", "see the next question", "covered in chapter X"
- No interview framing: no "a good answer would", "interviewers often ask", "make sure to mention"
- No trailing summaries or "the takeaway is" conclusions — end when the explanation is complete
- No repeated restatements of the question

## Execution Steps

1. Read the target file in full.
2. Identify every question block (headed by `## Q{N}.` or similar).
3. Preserve the file header (title line, Table of Contents if present, any `---` separators above the first question).
4. For each question:
   a. Keep the question heading unchanged.
   b. Remove the existing answer content entirely.
   c. Write a new `**Concepts**` section with one-line bullets.
   d. Write a new `**Answer**` section as flowing prose.
5. Write the reformatted content back to the same file.
6. Report how many questions were reformatted.
