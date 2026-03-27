###################################

# BACKEND

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

[working-directory: "backend/PayTrack.Tests"]
print-test-report:
    reportgenerator -reports:coverage.info -targetdir:coverage-report -reporttypes:Html
    @echo Generated report at backend/Paytack.Tests/coverage-report/index.html

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

###################################

# FRONTEND

[working-directory: "frontend"]
run-frontend:
    npm run start

[working-directory: "frontend"]
update-frontend:
    npm install

[working-directory: "frontend"]
build-frontend:
    npm run build -- --configuration production

[working-directory: "frontend"]
test-frontend:
    npm run test:coverage

[working-directory: "frontend"]
format-frontend:
    npx eslint . --ext .ts,.js,.html --fix

[working-directory: "frontend"]
lint-frontend:
    npx eslint . --max-warnings=0


##################################

# COMMON

[working-directory: "frontend"]
generate-api:
    npm run generate:api
