# Disclaimer

Before you are able to run the application you need to create a .env file. You can just copy the env.example and replace the dummy values with your production values. For now the only configuration you actually need to run the application is the GOOGLE\_CLIENT\_ID. You can get your own google client id by going to the Google Cloud Console and creating a new project. There you will get a client id specifically for you.

# PayTrack Backend

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
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

# Using SonarQube for Code Analysis

SonarQube is a great tool for analyzing your code and finding bugs/improvements/etc.

First you have to start SonarQube locally by running run-sonarqube. This will start sonarqube in a docker container.
You can then access the UI by going to http://localhost:9000 and logging in with "admin/admin".

Inside SonarQube you then need to create 2 new projects (call them something like "PayTrack\_Backend" and "PayTrack\_Fronted").

In those projects you will need to create a new Access Key (Can be found inthe Project Settings). Copy this key into a .env file
in the root folder of the project (there is a env.example file which you can copy). Once this is all set up you can run
"just sonar-backend" and "just sonar-frontend" which will run the sonarqube tool on the frontend and the backend.
Inside the UI you can then see all the Issues it detected with your code and fix them. For the backend it additionally 
runs all tests and outputs the test coverage and which files need more testing. Very useful!
