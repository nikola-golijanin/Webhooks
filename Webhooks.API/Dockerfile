FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5133

ENV ASPNETCORE_URLS=http://+:5133

USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["Webhooks.API/Webhooks.API.csproj", "Webhooks.API/"]
RUN dotnet restore "Webhooks.API/Webhooks.API.csproj"
COPY . .
WORKDIR "/src/Webhooks.API"
RUN dotnet build "Webhooks.API.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "Webhooks.API.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Webhooks.API.dll"]
