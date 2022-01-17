FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY . ./
RUN dotnet restore -s https://api.nuget.org/v3/index.json

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:5.0
ARG GIT_COMMIT=unspecified
LABEL git_commit=$GIT_COMMIT
WORKDIR /app
COPY --from=build-env /app/out .
ENV GIT_COMMIT $GIT_COMMIT
ENTRYPOINT ["dotnet", "BenefactAPI.dll"]
