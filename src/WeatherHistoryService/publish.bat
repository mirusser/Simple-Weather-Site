@RD /S /Q "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\WeatherHistoryService\deploy"
md C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\WeatherHistoryService
dotnet publish  -c Release -o "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\WeatherHistoryService\deploy"

docker build -t weatherhistoryservice .