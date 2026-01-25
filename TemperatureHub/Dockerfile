FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled AS base

WORKDIR /app
EXPOSE 5000


FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
WORKDIR /src
COPY ["TemperatureHub.csproj", ""]
RUN dotnet restore "TemperatureHub.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "TemperatureHub.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "TemperatureHub.csproj" -c Release -o /app
#RUN mkdir -p /app/AppData

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
VOLUME /app/AppData
ENV ClientId=ClientId
ENV ClientSecret=ClientSecret
ENV Username=Username
ENV Password=Password
ENTRYPOINT ["dotnet", "TemperatureHub.dll"]

#docker run -d -v C:\DockerVolume\SensorData:/app/AppData -p 5000:5000 ImageId
#docker run -d -v ~/SensorData:/app/AppData -p 5000:5000 --restart=always ImageId 