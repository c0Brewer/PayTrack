ARG SDK_IMAGE=mcr.microsoft.com/dotnet/sdk:10.0
ARG RUNTIME_IMAGE=mcr.microsoft.com/dotnet/aspnet:10.0

FROM ${SDK_IMAGE} AS build
WORKDIR /src

COPY . .
RUN dotnet restore ./PayTrack/PayTrack.csproj
RUN dotnet publish ./PayTrack/PayTrack.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM ${RUNTIME_IMAGE} AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "PayTrack.dll"]
