FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia o csproj da pasta dotnet/ para dentro do container
COPY ["dotnet/cloud-application.csproj", "./"]
RUN dotnet restore "cloud-application.csproj"

# Copia todo o restante do código da pasta dotnet/
COPY dotnet/ ./
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "cloud-application.dll"]