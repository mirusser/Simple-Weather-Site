# Simple Weather Site

Setup (writing setup in progress):
1. Get api key from: [openweathermap.org](https://openweathermap.org/) (getting this key may take up to few hours)
2. Create user secrets in WeatherService project: `dotnet user-secrets init` 
<br /> There should be this entry in WeatherService.csproj: 
<br /> `<UserSecretsId>some_guid</UserSecretsId>`
4. You can look up all your stored secrets with this command: `dotnet user-secrets list`
5. Set up your api key secret in WeatherService project: `dotnet user-secrets set "ServiceSettings:ApiKey" "your_api_key_here"`
