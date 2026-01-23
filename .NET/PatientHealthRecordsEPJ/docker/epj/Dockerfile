#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8443

ENV ASPNETCORE_URLS=http://*:8080

# Install time zone database (required for TimeZoneInfo on Alpine)
RUN apk add --no-cache tzdata

# (Optional) Set container default timezone for any code that still uses "local time"
# Your Razor fix should NOT rely on this, but it can prevent surprises elsewhere.
ENV TZ=Europe/Oslo

FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

COPY ./.NET/PatientHealthRecordsEPJ/solutions/PatientHealthRecordsEPJ/ ./

RUN dotnet restore "PatientHealthRecordsEPJ/PatientHealthRecordsEPJ.csproj"
RUN dotnet publish "PatientHealthRecordsEPJ/PatientHealthRecordsEPJ.csproj" -c Release -o /app/publish-documentsharing-epj /p:UseAppHost=false --no-restore 

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish-documentsharing-epj .

# Opt out of the diagnostic pipeline, allowing the container to run as read-only: https://learn.microsoft.com/en-us/dotnet/core/docker/build-container?tabs=windows#create-the-dockerfile
ENV DOTNET_EnableDiagnostics=0

ENTRYPOINT ["dotnet", "PatientHealthRecordsEPJ.dll"]
