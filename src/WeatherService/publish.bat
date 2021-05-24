@RD /S /Q "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\WeatherService\deploy"
md C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\WeatherService
dotnet publish  -c Release -o "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\WeatherService\deploy"

docker build -t weatherservice .