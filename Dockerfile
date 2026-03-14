# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["UserAuthApi.csproj", "."]
RUN dotnet restore "UserAuthApi.csproj"

COPY . .
RUN dotnet build "UserAuthApi.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "UserAuthApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UserAuthApi.dll"]
