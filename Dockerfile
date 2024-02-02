#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["CinemaApp.Api/CinemaApp.Api.csproj", "CinemaApp.Api/"]
COPY ["CinemaApp.Application/CinemaApp.Application.csproj", "CinemaApp.Application/"]
COPY ["CinemaApp.Application.Tests/CinemaApp.Application.Tests.csproj", "CinemaApp.Application.Tests/"]
COPY ["CinemaApp.Core/CinemaApp.Core.csproj", "CinemaApp.Core/"]
COPY ["CinemaApp.Infrastructure.Integrations/CinemaApp.Infrastructure.Integrations.csproj", "CinemaApp.Infrastructure.Integrations/"]
COPY ["CinemaApp.Infrastructure.Repositories/CinemaApp.Infrastructure.Repositories.csproj", "CinemaApp.Infrastructure.Repositories/"]
COPY ["CinemaApp.Utilities/CinemaApp.Utilities.csproj", "CinemaApp.Utilities/"]
RUN dotnet restore "CinemaApp.Api/CinemaApp.Api.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "CinemaApp.Api/CinemaApp.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CinemaApp.Api/CinemaApp.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Development
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CinemaApp.Api.dll"]