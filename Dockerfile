# Estágio de Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["dotnet/CloudApplication.csproj", "./"]
RUN dotnet restore "CloudApplication.csproj"

COPY dotnet/ .
RUN dotnet publish "CloudApplication.csproj" -c Release -o /app/publish

# Estágio Final / Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

ENTRYPOINT ["dotnet", "CloudApplication.dll"]
