FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore AspNetCoreDashboard.slnx
RUN dotnet publish Samples/AspNetCoreDashboardWebSamples/AspNetCoreDashboardWebSamples.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "AspNetCoreDashboardWebSamples.dll"]
