@RD /S /Q "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\Gateway\deploy"
md C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\Gateway
dotnet publish  -c Release -o "C:\Users\Jan Kowalski\Source\Repos\Simple-Weather-Site\src\Gateway\deploy"

docker build -t gateway .