@RD /S /Q "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\CitiesService\deploy"
md C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\CitiesService
dotnet publish  -c Release -o "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\CitiesService\deploy"

docker build -t citiesservice .