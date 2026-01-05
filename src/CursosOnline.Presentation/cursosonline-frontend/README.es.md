# CursosOnline – Backend y Frontend (ES)

**Repositorio GitHub:**
https://github.com/David-SilvaDEV/CursosOnline

Ejemplo sencillo de **arquitectura limpia** para una plataforma de cursos online, construido con **ASP.NET Core 8**, **Entity Framework Core (PostgreSQL)** y un frontend ligero en **Vite + JavaScript puro (Vanilla JS)**.

Incluye:

- ✅ API REST con **ASP.NET Core 8**
- ✅ Arquitectura limpia (Domain / Application / Infrastructure / Presentation)
- ✅ Autenticación con **ASP.NET Core Identity** + **JWT**
- ✅ Usuario por defecto creado al iniciar la API
- ✅ CRUD de cursos con paginación, filtros y publicar/despublicar
- ✅ Gestión de lecciones por curso (crear, listar, editar, eliminar lógico, reordenar)
- ✅ Frontend sencillo en HTML/CSS/JS que consume la API

---

## Tecnologías

**Backend**

- .NET 8 (ASP.NET Core Web API)
- Entity Framework Core 8
- Npgsql (proveedor PostgreSQL)
- ASP.NET Core Identity
- Swashbuckle (Swagger/OpenAPI)

**Frontend**

- Vite
- JavaScript (Vanilla)
- HTML y CSS

---

## Estructura del proyecto

```text path=/home/Coder/RiderProjects/CursosOnline/src/CursosOnline.Presentation/cursosonline-frontend/STRUCTURE.txt start=1
CursosOnline.sln
└── src/
    ├── CursosOnline.Application  # Lógica de aplicación y DTOs
    ├── CursosOnline.Domain       # Entidades de dominio (Course, Lesson, interfaces)
    ├── CursosOnline.Infrastructure  # EF Core, repositorios, migraciones, Identity
    └── CursosOnline.Presentation
        ├── CursosOnline.Api         # API ASP.NET Core (controladores, Program.cs)
        └── cursosonline-frontend    # Frontend Vite + JS + HTML + CSS
```

### Capa Domain (`CursosOnline.Domain`)

**Entidades:**

- `Course`
  - `Id` (GUID)
  - `Title` (string, requerido, máx. 150)
  - `Status` (`Draft` | `Published`)
  - `IsDeleted` (bool, borrado lógico)
  - `CreatedAt` (DateTime)
  - `UpdatedAt` (DateTime)
  - `ICollection<Lesson> Lessons`

- `Lesson`
  - `Id` (GUID)
  - `CourseId` (GUID, FK a `Course`)
  - `Title` (string)
  - `Order` (int, único por curso)
  - `IsDeleted` (bool)
  - `CreatedAt`, `UpdatedAt`

**Interfaces de repositorio:**

- `ICourseRepository`
  - `GetByIdAsync`
  - `GetAllAsync`
  - `GetPublishedAsync`
  - `AddAsync`, `UpdateAsync`, `SoftDeleteAsync`
  - `GetAllAsync` / `GetPublishedAsync`
- `ILessonRepository`
  - `GetByIdAsync`, `GetAllAsync`, `GetPublishedAsync`
  - `AddAsync`, `UpdateAsync`, `SoftDeleteAsync`
  - `move-up` / `move-down` (reordenar)

### Capa Application (`CursosOnline.Application`)

- **DTOs**
  - Cursos: `CourseCreateDto`, `CourseUpdateDto`, `CourseListDto`, `CourseResponseDto`, `CourseResponseDto`.
  - Clientes: `ClientRegisterDto`, `ClientLoginDto`, `ClientUpdateDto`, `ClientResponseDto`.
  - Respuestas estándar: `ApiResponseDto<T>`, `PagedResponseDto<T>`.

- **Reglas de negocio (en servicios/repositorios)**
  - El `Course.Status` se guarda como string.
  - Un curso solo puede publicarse (`/publish`) si tiene al menos una `Lesson` activa (no eliminada).
  - `IsDeleted` se usa para borrado lógico tanto en cursos como en lecciones.
  - `Lesson.Order` es único por curso y se usa para reordenar sin crear duplicados.

### Capa Infrastructure (`CursosOnline.Infrastructure`)

- `ApplicationDbContext` (hereda de `IdentityDbContext<IdentityUser, IdentityRole, string>`)
  - `DbSet<Course> Courses`, `DbSet<Lesson> Lessons`.
  - Filtros globales por `IsDeleted`.
  - Índice único `(CourseId, Order)` para las lecciones.
- Repositorios:
  - `CourseRepository` (CRUD + búsqueda, publicar/despublicar, soft delete).
  - `LessonRepository` (CRUD, listar por curso, borrar lógico, reordenar).
- Migraciones EF Core y configuración de PostgreSQL.

### Capa Presentation (`CursosOnline.Presentation/CursosOnline.Api`)

- `Program.cs`
  - Configura:
    - `AddDbContext<ApplicationDbContext>` (PostgreSQL)
    - `AddIdentity<IdentityUser, IdentityRole>` con reglas básicas de contraseña.
    - Autenticación JWT (Bearer).
    - CORS (en desarrollo: `AllowAnyOrigin/AnyHeader/AnyMethod`).
- `IdentityDataSeeder`
  - Crea un usuario por defecto al iniciar la API:
    - Email: `david@gmail.com`
    - Password: `David123*`
- Controladores principales:
  - `AuthController`
    - `POST /api/auth/register` – registro (sin autenticación).
    - `POST /api/auth/login` – login (devuelve JWT, sin autenticación).
  - `CoursesController` (todas las rutas con `[Authorize]`):
    - `GET    /api/courses/search?q=&status=&page=&pageSize=` – lista de cursos con filtros.
    - `POST   /api/courses` – crear curso.
    - `GET    /api/courses/{id}` – detalle de curso.
    - `PUT    /api/courses/{id}` – actualizar curso.
    - `DELETE /api/courses/{id}` – borrado lógico.
    - `PATCH  /api/courses/{id}/publish` – publicar curso (si tiene al menos una lección activa).
    - `PATCH  /api/courses/all/{id}/unpublish` – despublicar curso.
    - `GET    /api/courses/{id}/summary` – resumen del curso (info básica, nº lecciones, última modificación).
  - `Lessons` (endpoints en `CoursesController`/`LessonController`):
    - `GET    /api/courses/{id}/lessons` – lista de lecciones por curso (ordenadas por `Order`).
    - `POST   /api/courses/{id}/lessons` – crear lección.
    - `DELETE /api/courses/{id}/lessons/{lessonId}` – borrado lógico de lección.
    - `PATCH  /api/courses/{id}/lessons/{lessonId}/move-up` – subir posición.
    - `PATCH  /api/courses/{id}/lessons/{lessonId}/move-down` – bajar posición.

### Frontend (`src/CursosOnline.Presentation/cursosonline-frontend`)

- SPA sencilla con Vite + JavaScript (sin frameworks pesados).
- Usa `fetch` con cabecera `Authorization: Bearer <token>` para consumir la API.
- Funcionalidades:
  - **Autenticación**
    - Pantalla de login (email + contraseña) → `POST /api/auth/login`, guarda JWT en `localStorage`.
    - Botón de logout que limpia el token y oculta las vistas protegidas.
  - **Cursos**
    - Listado con paginación y filtro por estado (`Draft` / `Published`).
    - Crear curso (se crea como `Draft`).
    - Actualizar título y estado.
    - Publicar / despublicar curso.
    - Eliminación lógica del curso.
  - **Lecciones**
    - Listar lecciones por curso.
    - Crear nuevas lecciones.
    - Eliminar (soft delete).
    - Reordenar lecciones (mover arriba/abajo).

---

## Puesta en marcha

### Requisitos

- **.NET 8 SDK** instalado
- **Node.js** (v18+ recomendado) y **npm**
- **PostgreSQL** en local (BD creada, por ejemplo `gestorcursos`)

Asegúrate de que la cadena de conexión en `appsettings.json` es correcta:

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

### 1. Crear la base de datos y aplicar migraciones

```bash
cd /home/Coder/RiderProjects/CursosOnline/src
# Crear/actualizar esquema en PostgreSQL
DOTNET_ENVIRONMENT=Development dotnet ef database update -p CursosOnline.Infrastructure -s CursosOnline.Presentation/CursosOnline.Api
```

Al iniciar la API, se creará automáticamente el usuario por defecto:

- **Email:** `david@gmail.com`
- **Password:** `David123*`

### 2. Iniciar la API (backend)

```bash
cd /home/Coder/RiderProjects/CursosOnline/src/CursosOnline.Presentation/CursosOnline.Api
DOTNET_ENVIRONMENT=Development dotnet run --launch-profile http
```

La API quedará escuchando en:

- `http://localhost:5066`

Puedes comprobar que funciona entrando a:

- `http://localhost:5066/swagger`

### 3. Iniciar el frontend (Vite)

```bash
cd /home/Coder/RiderProjects/CursosOnline/src/CursosOnline.Presentation/cursosonline-frontend
npm install
npm run(dep)
```