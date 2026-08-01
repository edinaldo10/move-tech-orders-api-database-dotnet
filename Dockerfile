# Estágio de Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["CloudApplication.csproj", "./"]
RUN dotnet restore

COPY . .
RUN dotnet publish "CloudApplication.csproj" -c Release -o /app/publish

# Estágio Final / Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8000
ENV ASPNETCORE_URLS=http://0.0.0.0:8000

# ATENÇÃO: O nome da DLL gerada é em minúsculas para corresponder ao csproj
ENTRYPOINT ["dotnet", "CloudApplication.dll"]
