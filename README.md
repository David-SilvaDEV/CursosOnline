# CursosOnline – Backend & Frontend (EN)

A simple **clean architecture** example for an online courses platform, built with **ASP.NET Core 8**, **Entity Framework Core (PostgreSQL)**, and a lightweight **Vite + Vanilla JS** frontend.

It includes:

- ✅ REST API with ASP.NET Core 8
- ✅ Clean Architecture (Domain / Application / Infrastructure / Presentation)
- ✅ Authentication with **ASP.NET Core Identity** + **JWT**
- ✅ Default user seeded on startup
- ✅ Courses CRUD with pagination, filtering and publish/unpublish
- ✅ Lessons per course (CRUD + reordering + soft delete)
- ✅ Simple frontend (HTML/CSS/JS with Vite) that consumes the API

---

## Tech stack

**Backend**

- .NET 8 (ASP.NET Core Web API)
- Entity Framework Core 8
- Npgsql (PostgreSQL provider)
- ASP.NET Core Identity
- Swashbuckle (Swagger/OpenAPI)

**Frontend**

- Vite
- Vanilla JavaScript
- HTML & CSS

---

## Project structure

```text path=/home/Coder/RiderProjects/CursosOnline/STRUCTURE.txt start=1
CursosOnline.sln
└── src/
    ├── CursosOnline.Domain/          # Domain models (Course, Lesson, interfaces)
    ├── CursosOnline.Application/     # DTOs and application logic
    ├── CursosOnline.Infrastructure/  # EF Core context, repositories, migrations
    └── CursosOnline.Presentation/
        ├── CursosOnline.Api/         # ASP.NET Core Web API (controllers, Program.cs)
        └── cursosonline-frontend/    # Vite + JS + HTML + CSS frontend
```

### Domain layer (`CursosOnline.Domain`)

Entities:

- `Course`
  - `Id` (GUID)
  - `Title` (string, required, max 150)
  - `Status` (enum: `Draft`, `Published`)
  - `IsDeleted` (bool, soft delete)
  - `CreatedAt` (DateTime)
  - `UpdatedAt` (DateTime)
  - `ICollection<Lesson> Lessons`

- `Lesson`
  - `Id` (GUID)
  - `CourseId` (GUID, FK to `Course`)
  - `Title` (string)
  - `Order` (int, unique per course)
  - `IsDeleted` (bool, soft delete)
  - `CreatedAt`, `UpdatedAt`

Interfaces:

- `ICourseRepository`
  - `GetByIdAsync(id)`
  - `GetAllAsync()`
  - `GetPublishedAsync()`
  - `GetAllAsync()` / `GetPublishedAsync()`
  - `AddAsync`, `UpdateAsync`, `SoftDeleteAsync`
- `ILoadsRepository` *(if added)*

### Application layer (`CursosOnline.Application`)

- **DTOs**
  - `CourseCreateDto`, `CourseUpdateDto`, `CourseListDto`, `CourseResponseDto`
  - `ClientRegisterDto`, `ClientLoginDto`, `ClientUpdateDto`, `ClientResponseDto`
  - `ApiResponseDto<T>`, `PagedResponseDto<T>` (standard API responses)
  - `Client` and `Course` DTOs for the frontend

- **Business rules implemented via services + repositories** (see `CursoOnline.Infrastructure` repos):
  - Soft delete on courses and lessons.
  - Course can only be published if it has at least one **active** lesson.
  - `Lesson.Order` is unique per course and used for reordering.
  - Reordering does not create duplicate orders.

### Infrastructure layer (`CursosOnline.Infrastructure`)

- `ApplicationDbContext` (inherits `IdentityDbContext<IdentityUser, IdentityRole, string>`)
  - `DbSet<Course> Courses`
  - `DbSet<Lesson> Lessons`
  - Global query filters for `IsDeleted` on `Course` and `Lesson`.
  - `Course.Status` stored as `string`.
  - Unique index on `(CourseId, Order)` for `Lesson`.
- Repositories:
  - `CourseRepository` implements `ICourseRepository` (CRUD, search, publish/unpublish, soft delete).
  - `LessonRepository` implements `ILessonRepository` (CRUD, list by course, soft delete, reordering).
- EF Core migrations and PostgreSQL configuration.

### Presentation layer (`CursosOnline.Presentation/CursosOnline.Api`)

- `Program.cs`
  - Configures:
    - `AddDbContext<ApplicationDbContext>` (PostgreSQL)
    - `AddIdentity<IdentityUser, IdentityRole>` with basic password rules
    - JWT authentication (Bearer)
    - CORS (in dev: `AllowAnyOrigin/AnyHeader/AnyMethod`)
- `IdentityDataSeeder`
  - Creates default user on startup:
    - Email: `david@gmail.com`
    - Password: `David123*`
- Controllers:
  - `AuthController`
    - `POST /api/auth/register` (no auth) – register a new user.
    - `POST /api/auth/login` (no auth) – returns JWT token.
  - `CoursesController` (all routes require `[Authorize]`):
    - `GET    /api/courses/search?q=&status=&page=&pageSize=` – list courses with search + pagination.
    - `POST   /api/courses` – create course.
    - `GET    /api/courses/{id}` – get course details.
    - `PUT    /api/courses/{id}` – update course.
    - `DELETE /api/courses/{id}` – soft delete.
    - `PATCH  /api/courses/{id}/publish` – publish course (only if it has at least one active lesson).
    - `PATCH  /api/courses/{id}/unpublish` – unpublish course.
    - `GET    /api/courses/{id}/summary` – basic course info, total lessons, last update.
  - `CoursesController` uses `ICourseRepository` and `ILessonRepository`.
  - Default MVC `HomeController` (not used by the SPA, just default template).

### Frontend (`src/CursosOnline.Presentation/cursosonline-frontend`)

- Vite + Vanilla JS single page app.
- Uses `fetch` with `Authorization: Bearer <token>` headers to call the API.
- Main features:
  - **Auth**
    - Login form (email + password) → calls `POST /api/auth/login` and stores JWT in `localStorage`.
    - Logout button clears token and hides protected UI.
  - **Courses**
    - List courses with pagination and status filter (`Draft` / `Published`).
    - Create new course (starts as `Draft`).
    - Edit course (title + status) via API.
    – Publish / unpublish course via `PATCH /api/courses/{id}/publish` and `/unpublish`.
    - Soft delete course via `DELETE /api/courses/{id}`.
  - **Lessons**
    - For each course, open a "Lecciones" panel.
    - List lessons for the course ordered by `Order`.
    - Create new lesson (`POST /api/courses/{id}/lessons`).
    - Delete (soft delete) a lesson.
    - Reorder lessons using up/down buttons (`PATCH /move-up` / `move-down`).

---

## Running the project

### Prerequisites

- **.NET 8 SDK** installed
- **Node.js** (v18+ recommended) & **npm**
- **PostgreSQL** running locally (or accessible) with a database created, e.g. `gestorcursos`

Update `appsettings.json` in `CursosOnline.Api` if needed:

```json path=/home/Coder/RiderProjects/CursosOnline/src/CursosOnline.Presentation/CursosOnline.Api/appsettings.json start=1
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=gestorcursos;Username=david;Password=david123"
  },
  "Logging": { ... },
  "Jwt": {
    "Key": "<your-secret-key>",
    "Issuer": "CursosOnline.Api",
    "Audience": "CursosOnline.Frontend"
  }
}
```

### 1. Apply database migrations

```bash
cd src
# Create DB schema in PostgreSQL
DOTNET_ENVIRONMENT=Development dotnet ef database update -p CursosOnline.Infrastructure -s CursosOnline.Presentation/CursosOnline.Api
```

> **Note**: The `IdentityDataSeeder` will create a default user at API startup.
>
> - Email: `david@gmail.com`
> - Password: `David123*`

### 2. Run the backend API

```bash
cd src/CursosOnline.Presentation/CursosOnline.Api
# Use the HTTP profile (http://localhost:5066)
DOTNET_ENVIRONMENT=Development dotnet run --launch-profile http
```

The API will listen on:

- `http://localhost:5066`

You can verify it by opening:

- `http://localhost:5066/swagger` (Swagger UI)

### 3. Run the frontend

```bash
cd src/CursosOnline.Presentation/cursosonline-frontend
npm install
npm run dev
```

Then open the URL that Vite prints (usually `http://localhost:5173`).

### 4. Login and test

1. Go to `http://localhost:5173`.
2. Log in with:
   - **Email:** `david@gmail.com`
   - **Password:** `David123*`
3. After login, you should see the course dashboard. From there you can:
   - Create, list, edit, publish/unpublish and delete courses.
   - Open each course and manage its lessons (create, edit, delete, reorder).

---

## Notes & Next steps

- **Security**: CORS is currently configured with `AllowAnyOrigin()` for simplicity during development. For production, you should restrict it to your real frontend domain.
- **HTTPS**: `UseHttpsRedirection()` is disabled in `Program.cs` for easier local development with `http://localhost:5066`. In production, re-enable HTTPS and update the frontend URLs to use `https://...`.
- **Improvements** (ideas):
  - Add proper UI feedback (spinners, form validation, error messages).
  - Use a front-end framework (React/Vue/Angular) instead of plain JS if the project grows.
  - Add unit/integration tests for Application and API layers.
  - Add roles/permissions for different types of users (e.g., admin vs student).
