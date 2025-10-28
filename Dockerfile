# build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ./src ./src
COPY ./tests ./tests
RUN dotnet restore src/PayChase.Auth.API/PayChase.Auth.API.csproj
RUN dotnet publish src/PayChase.Auth.API/PayChase.Auth.API.csproj -c Release -o /app/publish

# run
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
# dotnet uses ASPNETCORE_URLS from env
EXPOSE 8080
ENTRYPOINT ["dotnet", "PayChase.Auth.API.dll"]
