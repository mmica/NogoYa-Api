# Frontend rules — Angular 18 + Tailwind + Material

Stack: **Angular 18 standalone components**, **Tailwind CSS 3**, **Angular
Material (M2 theme)**, **RxJS + Signals**, **TypeScript strict**.

Read these rules before writing any frontend code. The list is short on
purpose — every rule is non-negotiable unless the user asks otherwise.

## Component anatomy

- **Standalone components only.** Never declare a feature module.
- **Template lives in its own `.html` file** (`templateUrl: './x.component.html'`).
  Never use inline `template: \`...\``. The user reviews HTML separately.
- **Co-located files**: `name.component.ts` + `name.component.html`. Only add
  `name.component.scss` if Tailwind cannot do the job (very rare).
- **Selector** uses the `app-` prefix and kebab-case (`<app-store-list>`).
- **One concern per component.** If it grows past ~250 lines, split it.

## Styling

- **Tailwind is the styling engine.** Always reach for utilities first.
- **No custom CSS classes** invented "just because" (no `.my-card`, `.btn-x`).
- **No `.scss` / `.css` files** unless absolutely required (overriding Material
  internals, animations Tailwind cannot express, etc.). The global theme lives
  in `src/styles.scss`; do not create additional global stylesheets.
- **No `appearance="outline"` on `<mat-form-field>`.** The default look is
  customized globally in `styles.scss` (white bg, soft border, brand focus
  ring). Trust it.
- **Colors** come from the Tailwind palette declared in `tailwind.config.js`:
  - `brand-*` (sage olive) for primary actions, links, focus.
  - `stone-*` for neutral surfaces and text (warmer than `slate`).
  - `peach`, `honey`, `coral`, `sage` for accent tints on home features.
  - Semantic chips: `emerald` for OK / active, `rose` for danger, `amber` only
    for genuine warnings (low stock).
- **Rounded corners**: `rounded-2xl` for cards, `rounded-xl` for inputs/badges,
  `rounded-full` for chips and avatars.
- **Borders over shadows.** Prefer `border border-stone-200 hover:border-brand-300`
  to `shadow-sm hover:shadow-md`. Lighter, more modern look.

## Inputs and forms

- **Signal-based inputs**: `readonly product = input.required<Product>();`
  (Angular 18 `input()` / `input.required()`). Do not use `@Input()` decorator
  for new code. Migrate existing `@Input()` to signals when you touch the file.
- **Outputs**: `readonly saved = output<Product>();` (also signal-style).
- **Reactive forms only.** Never template-driven (`ngModel` for two-way bindings
  in a form). `ngModel` is fine in isolated cases (cart quantity input).
- **Use `FormBuilder.nonNullable.group(...)`.** No nullable form controls
  unless the field is genuinely optional.
- **Validators must produce Spanish error messages** in the template, e.g.
  `*ngIf="form.controls.name.hasError('required')"` shows "El nombre es
  obligatorio."

## State and reactivity

- **Signals for component state**: `protected readonly loading = signal(false);`.
- **`computed()`** for derived values (cart total, effective price preview).
- **`effect()`** sparingly — prefer pure `computed` + RxJS pipelines for async.
- **RxJS for HTTP**: services return `Observable<T>`. Components `subscribe`
  with explicit `{ next, error, complete }` blocks, never bare callbacks.
- **`takeUntil(destroy$)`** for streams that outlive a single request, OR
  `takeUntilDestroyed()` (modern Angular signal-aware operator).

## TypeScript

- **No `any`.** Anywhere. Use `unknown` and narrow if you must.
- **Every API response has a TypeScript interface** in `core/models/`. Never
  type a service return as `any` or `object`.
- **Generic constraints are explicit.** `<T extends BaseEntity>` not bare `<T>`.
- **`as` casts only when narrowing safely** (e.g. after a `typeof` guard).
  No `as any`, no `as unknown as X`.
- **`readonly` on injected dependencies** and on properties that are not
  reassigned: `private readonly http = inject(HttpClient);`.
- **Optional chaining + nullish coalescing**: `user?.name ?? 'Anónimo'`.

## Nothing hardcoded

- **No magic strings or numbers** in components. Pull them from:
  - `environment.ts` for things that change per deploy (API base URL).
  - A const block at the top of the component if it is purely UI (e.g. page
    size, debounce ms, default sort).
  - A backend config endpoint if it is business data.
- **API URLs**: services use relative paths (`'products'`, `'stores/search'`).
  The `apiInterceptor` prepends `environment.apiBaseUrl`. Never write the
  full URL inside a service.
- **Translated strings**: keep them inline in the template (Spanish text is
  part of the UI). i18n with `@angular/localize` is out of scope for this MVP.

## Project structure

```
core/
  models/        Interface per entity + payloads (Create*, Update*, *Filter)
  services/     One service per entity, thin HTTP layer
  interceptors/ apiInterceptor (base URL), errorInterceptor (snackbar)
  i18n/         MatPaginatorIntl etc.
features/
  <feature>/    Co-located .ts + .html, dialog components in same folder
layouts/        App shell (main-layout)
shared/         Cross-feature presentational components
```

- **One service per resource** (`StoreService`, `ProductService`, etc.).
- **Dialogs are components**, not inline `MatDialog` configs. File name:
  `<entity>-form-dialog.component.{ts,html}`.

## Material Angular

- **M2 theme** declared in `styles.scss`. Use `mat.m2-define-palette`, not M3
  experimental.
- **Sage palette** for primary, sage-light for accent. Never reach into
  `mat-grey` directly.
- **MatPaginator labels are global Spanish** via `paginator-intl.ts`. Don't
  re-translate per-component.
- **MatSnackBar for transient messages** (success, error). Duration 3-5s, with
  "Cerrar" action.

## Things to avoid

- `*ngIf`, `*ngFor`, `*ngSwitch` directives. Use **new control flow** `@if`,
  `@for (item of items; track item.id)`, `@switch`. Migrate when you touch.
- Subscribing without unsubscribing in long-lived components.
- Mutating signal values: always set a new array/object (`signal.set([...])`).
- Calling backend endpoints with `?pageSize=200` from the UI — every list view
  caps at 25.
- `console.log` left behind in committed code.

## When in doubt

- Use the existing pattern of a similar feature (e.g. mirror Stores admin when
  building a new admin page).
- Ask the user before introducing a new dependency.
