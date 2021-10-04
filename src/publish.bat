
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

@RD /S /Q "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\Gateway\deploy"
md "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\Gateway"
cd "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\Gateway"
dotnet publish  -c Release -o "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\Gateway\deploy"
docker build -t gateway .

@RD /S /Q "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\EmailService\deploy"
md "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\EmailService"
cd "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\EmailService"
dotnet publish  -c Release -o "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\EmailService\deploy"
docker build -t emailservice .

@RD /S /Q "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\WeatherSite\deploy"
md "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\WeatherSite"
cd "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\WeatherSite"
dotnet publish  -c Release -o "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\WeatherSite\deploy"
docker build -t weathersite .

pause