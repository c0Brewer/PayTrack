[working-directory: "backend"]
run-database:
    sudo docker-compose -f docker-compose-postgres.yml up -d

[working-directory: "backend"]
stop-database:
    sudo docker-compose -f docker-compose-postgres.yml down

[working-directory: "backend"]
run-backend:
    dotnet run --project PayTrack/PayTrack.csproj

[working-directory: "backend"]
test-backend:
    dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=lcov /p:Threshold=80 /p:ThresholdType=line /p:ThresholdStat=total

[working-directory: "backend"]
format-backend:
    dotnet format -v diagnostic

[working-directory: "backend"]
build-backend:
    dotnet build --configuration Release -warnaserror

[working-directory: "backend/PayTrack"]
create-migration name:
    dotnet ef migrations add {{name}}
    dotnet ef database update

[working-directory: "frontend"]
run-frontend:
    @echo "NOT IMPLEMENTED"

