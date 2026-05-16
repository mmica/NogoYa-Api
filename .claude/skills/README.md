# Skills

Reusable, opinionated instructions for tasks that come up over and over in
Nogo-Ya. Each skill lives in its own `.md` file with a focused recipe.

When a future Claude session is asked to do one of these tasks, it should
read the matching skill and follow it.

## Available skills

- [`scaffold-admin-page.md`](./scaffold-admin-page.md) — Build a new
  `/settings/<entity>` admin page (table + search + paginator + create/edit
  dialog), mirroring the Stores and Products implementations.

## Adding a new skill

1. Create `.claude/skills/<kebab-case-name>.md`.
2. Start with a one-sentence purpose.
3. List the inputs the user must provide.
4. Give a step-by-step recipe with file paths and code snippets.
5. End with "Verification" steps the user can run.
