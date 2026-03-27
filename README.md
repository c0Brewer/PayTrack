test
TODO: Properly implement README. For now its mostly instructions on how to run the program

# PayTrack Backend

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [Docker & Docker Compose](https://docs.docker.com/compose/) (for PostgreSQL)
- `just` command (optional, for running predefined tasks)
- EF Core CLI for migrations: `dotnet tool install --global dotnet-ef`
- [`dotnet-reportgenerator-globaltool`](https://github.com/danielpalme/ReportGenerator) (optional, for generating HTML coverage reports)

## Commands

### Running the Database

Start the PostgreSQL database:

```bash
just run-database
```

Stop the database:

```bash
just stop-database
```

> This uses `docker-compose-postgres.yml` to start/stop the database in the background.

### Running the Backend

Run the backend API:

```bash
just run-backend
```

- Runs the project located at `PayTrack/PayTrack.csproj`.
- The API will connect to the PostgreSQL database running in Docker.

### Building the Backend

Build the backend in release mode:

```bash
just build-backend
```

- Treats all warnings as errors (`-warnaserror`), same as the CI/CD pipeline.

### Testing the Backend

Run unit tests with code coverage:

```bash
just test-backend
```

- Uses Coverlet to collect coverage in **OpenCover** format.
- Enforces a minimum **line coverage threshold of 80%**.
- Coverage reports can be converted to **HTML** using:

```bash
just print-test-report
```

- Opens a detailed report at `backend/PayTrack.Tests/coverage-report/index.html`.
- Lines not covered will be highlighted in red; covered lines in green.

> Note: Tests are configured to run without requiring a real database where possible. Services are mocked when needed.
> We might add libraries like **Testcontainers** to run tests in isolated environments.

### Formatting the Code

Automatically format C# code according to `.editorconfig` and StyleCop rules:

```bash
just format-backend
```

### Database Migrations

Create a new migration and apply it:

```bash
just create-migration <MigrationName>
```

- Replace `<MigrationName>` with a descriptive name for the migration.
- Example:

```bash
just create-migration AddTeamEntity
```

- This will:
  1. Create a new EF Core migration.
  2. Apply it to the database.

## Notes

- Ensure Docker is running before starting the database.
- The database is required for the application to properly run.
- The backend exposes minimal API endpoints via `Program.cs`.

# Frontend

The Frontend is an Angular Application.

## Commands

### Starting the Frontend

```bash
just run-frontend
```

- Installs dependencies and starts the Angular development server.

### Testing the Frontend

```bash
just test-frontend
```

- Runs tests and generates coverage reports (configured in the Angular project).

### Linting the Frontend

Runs the linter for code quality checks:

```bash
just lint-frontend
```

### Formatting the Frontend

Automatically fixes formatting issues:

```bash
just format-frontend
```

### Building the Frontend

```bash
just build-frontend
```

- Builds the Angular project in production mode.

### Generating API Clients

```bash
just generate-api
```

- Runs the frontend script to regenerate API clients from the backend OpenAPI specification.
