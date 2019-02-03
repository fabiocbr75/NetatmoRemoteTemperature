docker rmi sensordata
docker rmi dashboard

cd TemperatureHub
docker build -t sensordata .
cd ..
docker save sensordata -o sensordata.tar

cd dashboard
docker build -t dashboard .
cd ..

docker save dashboard -o dashboard.tar

docker images | ConvertFrom-String | where {$_.P2 -eq "<none>"} | % { docker rmi $_.P3 }