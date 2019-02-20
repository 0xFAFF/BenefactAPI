FROM microsoft/dotnet:sdk AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore -s http://vector57.net:5000/v3/index.json -s https://api.nuget.org/v3/index.json

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM microsoft/dotnet:aspnetcore-runtime
ARG GIT_COMMIT=unspecified
LABEL git_commit=$GIT_COMMIT
WORKDIR /app
COPY --from=build-env /app/out .
ENV GIT_COMMIT $GIT_COMMIT
ENTRYPOINT ["dotnet", "BenefactAPI.dll"]
