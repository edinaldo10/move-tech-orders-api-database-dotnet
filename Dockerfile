FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia e restaura o projeto principal
COPY ["dotnet/cloud-application.csproj", "./"]
RUN dotnet restore "cloud-application.csproj"

# Copia o restante do código fonte da pasta dotnet para a raiz do container
COPY dotnet/ .
RUN dotnet publish "cloud-application.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "cloud-application.dll"]