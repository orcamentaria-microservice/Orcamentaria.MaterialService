FROM mcr.microsoft.com/dotnet/sdk:8.0 as build
WORKDIR /src

COPY ["Orcamentaria.MaterialService.API/Orcamentaria.MaterialService.API.csproj", "Orcamentaria.MaterialService.API/"]
COPY ["Orcamentaria.MaterialService.Application/Orcamentaria.MaterialService.Application.csproj", "Orcamentaria.MaterialService.Application/"]
COPY ["Orcamentaria.MaterialService.Domain/Orcamentaria.MaterialService.Domain.csproj", "Orcamentaria.MaterialService.Domain/"]
COPY ["Orcamentaria.MaterialService.Infrastructure/Orcamentaria.MaterialService.Infrastructure.csproj", "Orcamentaria.MaterialService.Infrastructure/"]

COPY nuget.config ./
COPY local-packages ./local-packages

RUN dotnet restore "Orcamentaria.MaterialService.API/Orcamentaria.MaterialService.API.csproj"

COPY . .

WORKDIR "/src/Orcamentaria.MaterialService.API"
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Orcamentaria.MaterialService.API.dll"]
# ENV ASPNETCORE_URLS=http://+:5000
# EXPOSE 5000