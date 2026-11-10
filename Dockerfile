# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/Pedidos360.Web/Pedidos360.Web.csproj", "src/Pedidos360.Web/"]
RUN dotnet restore "src/Pedidos360.Web/Pedidos360.Web.csproj"

COPY . .
WORKDIR "/src/src/Pedidos360.Web"
RUN dotnet publish "Pedidos360.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage. Render supplies PORT at runtime.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000
ENTRYPOINT ["sh", "-c", "dotnet Pedidos360.Web.dll --urls http://0.0.0.0:${PORT:-10000}"]
