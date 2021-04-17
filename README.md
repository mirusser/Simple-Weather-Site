# Simple Weather Site

Setup (writing setup in progress):
1. Get api key from: [openweathermap.org](https://openweathermap.org/) (getting this key may take up to few hours)
2. Create user secrets in WeatherService project: `dotnet user-secrets init` 
<br /> There should be this entry in WeatherService.csproj: 
<br /> `<UserSecretsId>some_guid</UserSecretsId>`
4. You can look up all your stored secrets with this command: `dotnet user-secrets list`
5. Set up your api key secret in WeatherService project: `dotnet user-secrets set "ServiceSettings:ApiKey" "your_api_key_here"`

TODO: 
- Implement proper exception handling in each microservice and weather site
- In each microservice add proper logging (error, warning) using seq I think
- Weather site: make proper user interface, add messages/alerts to user on what is going on (alerts on success/failure) and proper visualisation of weather forecast (like proper images and visualisation of data)
- Implement Onion Architecture for at least WeatherSite (for microservices it would be overkill)
- Implement proper healthchecks for each microservice, and implement hangfire job in weather site that will check health of each microservice from time to time, and if any will be unhealthy send mail to me with info about it
