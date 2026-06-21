ARG SDK_IMAGE=mcr.microsoft.com/dotnet/sdk:10.0-alpine
ARG RUNTIME_IMAGE=mcr.microsoft.com/dotnet/aspnet:10.0-alpine

FROM ${SDK_IMAGE} AS build
WORKDIR /src

COPY . .
RUN dotnet restore ./PayTrack/PayTrack.csproj
RUN dotnet publish ./PayTrack/PayTrack.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM ${RUNTIME_IMAGE} AS runtime
WORKDIR /app

RUN apk add --no-cache \
    icu-data-full \
    icu-libs \
    poppler-utils \
    tesseract-ocr \
    tesseract-ocr-data-deu \
    tesseract-ocr-data-eng

ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "PayTrack.dll"]
