# Usar la imagen oficial de .NET 9.0
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["examen parcial.csproj", "."]
RUN dotnet restore "examen parcial.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "examen parcial.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "examen parcial.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Crear directorio para la base de datos
RUN mkdir -p /app/data

# Configurar variables de entorno
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "examen parcial.dll"]