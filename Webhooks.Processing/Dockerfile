FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5153

ENV ASPNETCORE_URLS=http://+:5153

USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["Webhooks.Processing/Webhooks.Processing.csproj", "Webhooks.Processing/"]
RUN dotnet restore "Webhooks.Processing/Webhooks.Processing.csproj"
COPY . .
WORKDIR "/src/Webhooks.Processing"
RUN dotnet build "Webhooks.Processing.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "Webhooks.Processing.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Webhooks.Processing.dll"]
