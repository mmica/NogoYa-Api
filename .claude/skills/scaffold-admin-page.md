# Skill: scaffold a new admin page

**Purpose**: Build a new `/settings/<entity>` page following the same pattern as
Stores and Products — table with search, paginator (cap 25), create/edit
dialog, soft-delete from row menu.

## Inputs the user must provide

- Entity name in PascalCase singular (e.g. `Order`, `Customer`).
- Plural English route slug (`orders`, `customers`).
- Plural Spanish UI label (`Pedidos`, `Clientes`).
- Columns to display in the table.
- Search fields (which entity properties full-text-search against).

## Backend recipe

1. **Filter DTO** in `NogoYa.Application/DTOs/<Entity>Dtos.cs`:

   ```csharp
   public record <Entity>FilterDto(
       string? Search,
       /* domain-specific filters */
       int Page = 1,
       int PageSize = 25);
   ```

2. **Repository** in `NogoYa.Infrastructure/Persistence/Repositories/<Entity>Repository.cs`:
   add `SearchAsync(filter, ct)` returning `PagedResult<<Entity>>`. Use
   `EF.Functions.ILike` for case-insensitive search.

3. **Repository interface** in `Application/Interfaces/Repositories/I<Entity>Repository.cs`:
   add `SearchAsync` to the contract.

4. **Service** in `Application/Services/<Entity>Service.cs`:
   - Add `const int MaxPageSize = 25;`.
   - Implement `SearchAsync` with the defensive page-size cap pattern from
     `StoreService.SearchAsync`.

5. **Service interface** in `Application/Interfaces/Services/I<Entity>Service.cs`:
   add `SearchAsync`.

6. **Controller** in `API/Controllers/<Entity>sController.cs`:

   ```csharp
   [HttpGet("search")]
   public async Task<IActionResult> Search([FromQuery] <Entity>FilterDto filter, CancellationToken ct)
       => (await _service.SearchAsync(filter, ct)).ToActionResult();
   ```

## Frontend recipe

1. **Model** `frontend/src/app/core/models/<entity>.model.ts`:
   add `<Entity>Filter`, `Create<Entity>Payload`, `Update<Entity>Payload`.

2. **Service** `frontend/src/app/core/services/<entity>.service.ts`:
   add `search`, `create`, `update`, `delete` methods following
   `StoreService` shape.

3. **Dialog** `frontend/src/app/features/settings/<entity>s/<entity>-form-dialog.component.{ts,html}`:
   - Standalone component.
   - Reactive form with FluentBuilder + nonNullable group.
   - Slug-like fields disabled in edit mode.
   - Use Material form fields (no `appearance="outline"`).

4. **List** `frontend/src/app/features/settings/<entity>s/<entity>s.component.{ts,html}`:
   - Mirror `StoresComponent` / `ProductsAdminComponent`.
   - `MatTable` with sticky header, hover state.
   - Filter bar with `bg-brand-50/40` tint.
   - `MatPaginator` with `[pageSizeOptions]="[25]"`.
   - Row menu: edit + soft-delete with confirmation.

5. **Wire up navigation**:
   - Add route in `app.routes.ts` under the main layout.
   - Add entry in `settings.component.ts` `entries` array.
   - Add menu item in `layouts/main-layout/main-layout.component.html`.

## Verification

- Backend: `dotnet build` clean. New `GET /api/v1/<entity>s/search` listed in
  Swagger. Returns `PagedResult` with 25-capped page size even when client
  asks for more.
- Frontend: `npm run build` clean. The new `/settings/<entity>s` page lists
  results, search debounces 300ms, paginator shows "Artículos por página: 25"
  and "1 – N de M".
- Create / edit / delete flow works end-to-end with a sample record.
