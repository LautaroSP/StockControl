FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY src/StockControl.Dominio/StockControl.Dominio.csproj src/StockControl.Dominio/
COPY src/StockControl.Api/StockControl.Api.csproj src/StockControl.Api/
RUN dotnet restore src/StockControl.Api/StockControl.Api.csproj
COPY src/StockControl.Dominio/ src/StockControl.Dominio/
COPY src/StockControl.Api/ src/StockControl.Api/
RUN dotnet publish src/StockControl.Api/StockControl.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
RUN mkdir -p /claves && chown $APP_UID /claves
USER $APP_UID
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "StockControl.Api.dll"]
