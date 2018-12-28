FROM microsoft/dotnet:2.2-aspnetcore-runtime-stretch-slim-arm32v7 AS base
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
RUN mkdir -p /app/AppData
VOLUME /app/AppData
ENV ClientId ClientId
ENV ClientSecret ClientSecret
ENV Username Username
ENV Password Password
ENTRYPOINT ["dotnet", "TemperatureHub.dll"]

#docker run -d -v C:\DockerVolume\SensorData:/app/AppData -p 5000:5000 ImageId
#docker run -d -v ~/SensorData:/app/AppData -p 5000:5000 --restart=always ImageId 