# Estágio de Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["cloud-application.csproj", "./"]
RUN dotnet restore

COPY . .
RUN dotnet publish "cloud-application.csproj" -c Release -o /app/publish

# Estágio Final / Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8000
ENV ASPNETCORE_URLS=http://0.0.0.0:8000

# ATENÇÃO: O nome da DLL gerada é em minúsculas para corresponder ao csproj
ENTRYPOINT ["dotnet", "cloud-application.dll"]