docker run -itd -p 80:80 -e ConnectionStrings:BenefactDatabase='Host=192.168.1.2;Username=docker;Database=benefact;Password=docker;' --name benefact benefact
docker network connect --ip 192.168.1.3 dbnetwork benefact
