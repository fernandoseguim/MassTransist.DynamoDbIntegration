FROM mcr.microsoft.com/dotnet/core/runtime:2.2-stretch-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["tests/MassTransist.DynamoDbIntegration.Tests/MassTransist.DynamoDbIntegration.Tests.csproj", "tests/MassTransist.DynamoDbIntegration.Tests/"]
RUN dotnet restore "tests/MassTransist.DynamoDbIntegration.Tests/MassTransist.DynamoDbIntegration.Tests.csproj"
COPY . .
WORKDIR "/src/tests/MassTransist.DynamoDbIntegration.Tests"
RUN dotnet build "MassTransist.DynamoDbIntegration.Tests.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MassTransist.DynamoDbIntegration.Tests.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MassTransist.DynamoDbIntegration.Tests.dll"]