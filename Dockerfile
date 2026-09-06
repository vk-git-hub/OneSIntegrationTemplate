FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/OneSIntegration.Api/OneSIntegration.Api.csproj ./OneSIntegration.Api/
RUN dotnet restore ./OneSIntegration.Api/OneSIntegration.Api.csproj
COPY src/OneSIntegration.Api/ ./OneSIntegration.Api/
WORKDIR /src/OneSIntegration.Api
RUN dotnet publish ./OneSIntegration.Api.csproj -c Release -o /app /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
COPY --from=build /app .
ENTRYPOINT ["dotnet", "OneSIntegration.Api.dll"]
