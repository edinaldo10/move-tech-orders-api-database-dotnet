# Estágio de Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia apenas o projeto principal e restaura
COPY ["cloud-application.csproj", "./"]
RUN dotnet restore "cloud-application.csproj"

# Copia o restante do código fonte da API (ignorando a pasta tests se houver .dockerignore)
COPY . .

# Publica explicitamente apenas o projeto principal da API
RUN dotnet publish "cloud-application.csproj" -c Release -o /app/publish

# Estágio Final / Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8000
ENV ASPNETCORE_URLS=http://0.0.0.0:8000

ENTRYPOINT ["dotnet", "CloudApplication.dll"]
