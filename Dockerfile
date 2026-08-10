FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia o csproj de dentro da pasta dotnet/
COPY ["dotnet/cloud-application.csproj", "./"]
RUN dotnet restore "cloud-application.csproj"

# Copia todo o código fonte da pasta dotnet/ para o container
COPY dotnet/ ./
RUN dotnet publish "cloud-application.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "cloud-application.dll"]