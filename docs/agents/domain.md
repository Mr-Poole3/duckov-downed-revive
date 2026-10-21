# Domain Docs

How engineering skills consume this repository's domain documentation.

## Before exploring

- Read `CONTEXT.md` at the repository root.
- Read ADRs under `docs/adr/` that touch the area being changed.

This is a single-context repository. If `CONTEXT.md` or an ADR does not exist
yet, proceed silently and create documentation lazily when decisions are
actually resolved.

## Use the glossary vocabulary

When naming domain concepts in issues, tests, refactor proposals, or code
interfaces, use the terms defined in `CONTEXT.md`. Do not invent synonyms when
an established term exists.

## Flag ADR conflicts

If a proposed change contradicts an existing ADR, surface the conflict
explicitly instead of silently overriding it.
