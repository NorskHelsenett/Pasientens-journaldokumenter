#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base

ARG NHN_HTTP_PROXY
ARG NHN_NO_PROXY

WORKDIR /app
EXPOSE 8080
EXPOSE 8443

ENV ASPNETCORE_URLS=http://*:8080

# Install time zone database (required for TimeZoneInfo on Alpine)
RUN HTTP_PROXY=${NHN_HTTP_PROXY} \
    HTTPS_PROXY=${NHN_HTTP_PROXY} \
    NO_PROXY=${NHN_NO_PROXY} \
    http_proxy=${NHN_HTTP_PROXY} \
    https_proxy=${NHN_HTTP_PROXY} \
    no_proxy=${NHN_NO_PROXY} \
    apk add --no-cache tzdata ca-certificates

# Trust the internal NHN PSKY root CA so outbound HTTPS calls to internal services
# (e.g. api-dev.repository.pjd.test.nhn.no) don't fail with secure_connection_error.
COPY ./.NET/PatientHealthRecordsEPJ/docker/epj/certs/nhn-internal-root-ca.crt /usr/local/share/ca-certificates/nhn-internal-root-ca.crt
RUN update-ca-certificates

# (Optional) Set container default timezone for any code that still uses "local time"
# Your Razor fix should NOT rely on this, but it can prevent surprises elsewhere.
ENV TZ=Europe/Oslo

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build

# Prepare the docker build arguments that contains the proxy settings:
ARG NHN_HTTP_PROXY
ARG NHN_NO_PROXY
# Set the environment proxy variables from the build arguments.
ENV HTTP_PROXY=${NHN_HTTP_PROXY}
ENV HTTPS_PROXY=${NHN_HTTP_PROXY}
ENV NO_PROXY=${NHN_NO_PROXY}
ENV http_proxy=${NHN_HTTP_PROXY}
ENV https_proxy=${NHN_HTTP_PROXY}
ENV no_proxy=${NHN_NO_PROXY}

WORKDIR /src

COPY ./.NET/PatientHealthRecordsEPJ/solutions/PatientHealthRecordsEPJ/ ./

RUN dotnet restore "PatientHealthRecordsEPJ/PatientHealthRecordsEPJ.csproj"
RUN dotnet publish "PatientHealthRecordsEPJ/PatientHealthRecordsEPJ.csproj" -c Release -o /app/publish-documentsharing-epj /p:UseAppHost=false --no-restore 

FROM base AS final

# Clear out the environment variables for our runtime stage
ENV HTTP_PROXY= HTTPS_PROXY= NO_PROXY= http_proxy= https_proxy= no_proxy=

WORKDIR /app
COPY --from=build /app/publish-documentsharing-epj .

# Opt out of the diagnostic pipeline, allowing the container to run as read-only: https://learn.microsoft.com/en-us/dotnet/core/docker/build-container?tabs=windows#create-the-dockerfile
ENV DOTNET_EnableDiagnostics=0

# Run as the non-root user built into the .NET 8+ base images, required by the cluster's Pod Security policies.
USER $APP_UID

ENTRYPOINT ["dotnet", "PatientHealthRecordsEPJ.dll"]
