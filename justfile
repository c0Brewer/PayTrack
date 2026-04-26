###################################
set windows-shell := ["powershell", "-Command"]

set dotenv-load

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
    dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=lcov /p:Threshold=90 /p:ThresholdType=line /p:ThresholdStat=total /p:ExcludeByFile="**/*Migrations/**.cs"

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

[working-directory: "backend"]
sonar-backend:
    dotnet sonarscanner begin /k:$SONAR_PROJECT_NAME_BACKEND /d:sonar.host.url="http://localhost:9000" /d:sonar.token=$SONARQUBE_TOKEN_BACKEND /d:sonar.exclusions="**/bin/**,**/obj/**,**/Migrations/**,**/*.generated.cs,**/coverage-report/**"  /d:sonar.cs.opencover.reportsPaths="**/coverage.opencover.xml"
    dotnet build
    dotnet test /p:CollectCoverage=true /p:Threshold=0 /p:CoverletOutputFormat=opencover /p:CoverletOutput=./TestResults/coverage.opencover.xml /p:ExcludeByFile="**/*Migrations/**.cs"
    dotnet sonarscanner end /d:sonar.token=$SONARQUBE_TOKEN_BACKEND

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

[working-directory: "frontend"]
sonar-frontend:
    npm run test:coverage:sonar
    sonar-scanner -Dsonar.host.url=http://localhost:9000 -Dsonar.token=$SONARQUBE_TOKEN_FRONTEND -Dsonar.projectKey=$SONAR_PROJECT_NAME_FRONTEND


##################################

# COMMON

[working-directory: "frontend"]
generate-api:
    npm run generate:api

[unix]
run-sonarqube:
    sudo docker-compose -f docker-compose-sonarqube.yml up -d

[windows]
run-sonarqube:
    docker-compose -f docker-compose-sonarqube.yml up -d

[unix]
stop-sonarqube:
    sudo docker-compose -f docker-compose-sonarqube.yml down

[windows]
stop-sonarqube:
    sudo docker-compose -f docker-compose-sonarqube.yml down

