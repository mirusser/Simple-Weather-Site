@RD /S /Q "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\WeatherSite\deploy"
md C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\WeatherSite
dotnet publish  -c Release -o "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\WeatherSite\deploy"

docker build -t weathersite .