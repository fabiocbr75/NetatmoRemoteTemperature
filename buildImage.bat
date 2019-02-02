docker image prune -a

docker rmi sensordata
docker rmi dashboard

cd TemperatureHub
docker build -t sensordata .
cd ..\dashboard
docker build -t dashboard .
cd ..
docker save sensordata > sensordata.tar
docker save dashboard > dashboard.tar