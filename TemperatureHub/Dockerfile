FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 5000

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["TemperatureHub.csproj", ""]
RUN dotnet restore "TemperatureHub.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "TemperatureHub.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "TemperatureHub.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "TemperatureHub.dll"]