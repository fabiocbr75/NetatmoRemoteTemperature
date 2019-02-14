docker rm $(docker stop $(docker ps -a -q --filter ancestor=dashboard --format="{{.ID}}"))
docker rmi dashboard
docker load < dashboard.tar
docker run -d -p 8080:80 -e TZ=Europe/Rome --restart=always dashboard
rm dashboard.tar

