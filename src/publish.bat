
docker network create -d bridge overlaynetwork

@RD /S /Q "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\CitiesService\CitiesGrpcService\deploy"
md "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\CitiesService\CitiesGrpcService"
cd "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\CitiesService\CitiesGrpcService"
dotnet publish  -c Release -o "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\CitiesService\CitiesGrpcService\deploy"
docker build -t citiesgrpcservice .

@RD /S /Q "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\CitiesService\CitiesService\deploy"
md "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\CitiesService\CitiesService"
cd "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\CitiesService\CitiesService"
dotnet publish  -c Release -o "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\CitiesService\CitiesService\deploy"
docker build -t citiesservice .

@RD /S /Q "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\WeatherService\deploy"
md "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\WeatherService"
cd "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\WeatherService"
dotnet publish  -c Release -o "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\WeatherService\deploy"
docker build -t weatherservice .

@RD /S /Q "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\WeatherHistoryService\deploy"
md "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\WeatherHistoryService"
cd "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\WeatherHistoryService"
dotnet publish  -c Release -o "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\WeatherHistoryService\deploy"
docker build -t weatherhistoryservice .

@RD /S /Q "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\EmailService\deploy"
md "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\EmailService"
cd "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\EmailService"
dotnet publish  -c Release -o "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\EmailService\deploy"
docker build -t emailservice .

@RD /S /Q "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\IconService\deploy"
md "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\IconService"
cd "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\IconService"
dotnet publish  -c Release -o "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\IconService\deploy"
docker build -t iconservice .

@RD /S /Q "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\SignalRServer\deploy"
md "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\SignalRServer"
cd "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\SignalRServer"
dotnet publish  -c Release -o "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\SignalRServer\deploy"
docker build -t signalrserver .

@RD /S /Q "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\WeatherSite\deploy"
md "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\WeatherSite"
cd "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\WeatherSite"
dotnet publish  -c Release -o "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\WeatherSite\deploy"
docker build -t weathersite .

docker-compose build

docker-compose up -d

pause