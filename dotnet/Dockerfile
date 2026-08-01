# Estágio de Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Como o contexto apontado no workflow é a pasta dotnet, 
# o csproj está na raiz do contexto (./CloudApplication.csproj)
COPY ["CloudApplication.csproj", "./"]
RUN dotnet restore "CloudApplication.csproj"

# Copia o restante dos arquivos da aplicação
COPY . .
RUN dotnet publish "CloudApplication.csproj" -c Release -o /app/publish

# Estágio Final / Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

ENTRYPOINT ["dotnet", "CloudApplication.dll"]
