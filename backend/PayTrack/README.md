# Simple .NET Application With PostgreSQL

Start with `dotnet run`


### Migrations

You need ef:

`dotnet tool install --global dotnet-ef`

Inital Creation of Migrations: 

`dotnet ef migrations add InitialCreate`

To update database:

`dotnet ef database update`

#### Workflow with Migrations:

After changing models:

`dotnet ef migrations add SomeChangeName`

The migrations is done automatically when starting
