# ── Stage 1: Build ────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

COPY Directory.Build.props global.json ./
COPY ["src/Birdie69.Api/Birdie69.Api.csproj", "src/Birdie69.Api/"]
COPY ["src/Birdie69.Application/Birdie69.Application.csproj", "src/Birdie69.Application/"]
COPY ["src/Birdie69.Infrastructure/Birdie69.Infrastructure.csproj", "src/Birdie69.Infrastructure/"]
COPY ["src/Birdie69.Domain/Birdie69.Domain.csproj", "src/Birdie69.Domain/"]
RUN dotnet restore "src/Birdie69.Api/Birdie69.Api.csproj"

COPY . .
RUN dotnet publish "src/Birdie69.Api/Birdie69.Api.csproj" \
    -c Release \
    -o /app/publish

# ── Stage 2: Runtime ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app

RUN addgroup -S appgroup && adduser -S appuser -G appgroup
USER appuser

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "Birdie69.Api.dll"]
