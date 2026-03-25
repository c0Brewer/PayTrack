TODO: Properly implement README. For now its mostly instructions on how to run the program

# PayTrack Backend

## Prerequisites

* [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
* [Docker & Docker Compose](https://docs.docker.com/compose/) (for PostgreSQL)
* `just` command (optional, for running predefined tasks)
* ef for migrations: `dotnet tool install --global dotnet-ef`

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

* This runs the project located at `PayTrack/PayTrack.csproj`.
* The API will connect to the PostgreSQL database running in Docker.

### Building the Backend

Build the backend in release mode (This is important when you want to test if the pipeline will go through)

```bash
just build-backend
```

* Treats all warnings as errors (`-warnaserror`). Same as the pipeline.

### Testing the Backend

Run unit tests with code coverage:

```bash
just test-backend
```

* Uses Coverlet to collect coverage in **lcov** format.
* Enforces a minimum **line coverage threshold of 80%**.

> Note: Tests are configured to run without requiring a real database where possible. Services are mocked when needed. 

We might have to add a library like "Testcontainers" which allows us to test our application in a cut-off test environment.

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

* Replace `<MigrationName>` with a descriptive name for the migration.
* Example:

```bash
just create-migration AddTeamEntity
```

* This will:

  1. Create a new EF Core migration.
  2. Apply it to the database.


## Notes

* Ensure Docker is running before starting the database.
* The databse is required for the Application to properly run
* The backend exposes minimal API endpoints via `Program.cs`.

# Frontend

The Frontend is an Angular Application.

## Commands

### Starting the Frontend

It can be run using

```bash
just run-frontend
```

### Testing the Frontend

```bash
just test-frontend
```

### Linting the Frontend

Runs the linter for checks

```bash
just lint-frontend
```

### Building the Frontend

```bash
just build-frontend
```
