docker rm $(docker stop $(docker ps -a -q --filter ancestor=sensordata --format="{{.ID}}"))
docker rmi sensordata
docker load < sensordata.tar
docker run -d -v ~/SensorData:/app/AppData  -p 5000:5000 -e TZ=Europe/Rome -e AppSettings:clientId='' -e AppSettings:clientSecret='' -e AppSettings:username='' -e AppSettings:password='' -e AppSettings:homeId='' --restart=always sensordata
rm sensordata.tar

