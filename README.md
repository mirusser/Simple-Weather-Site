# Simple Weather Site

### Current state of the project

- Migrated from .NET 5 to .NET 8
- Changed folder structure to be more usable and 'friendly'
- Fixed general errors that prevented project from being fully operational when running on Docker
- Some minor refactor
- Still there is a lot of refactoring to do till I'm gonna be at least pleased with the state of the code

### TODOs:

- general refactor
- Implement proper exception handling in each microservice and weather site
- In each microservice add proper logging (error, warning) using seq I think
- Weather site: make proper user interface, add messages/alerts to user on what is going on (alerts on success/failure) and proper visualisation of weather forecast (like proper images and visualisation of data)
- Implement proper healthchecks for each microservice, and implement hangfire job in weather site that will check health of each microservice from time to time, and if any will be unhealthy send mail to me with info about it (add microservice for sending emails?)
  test test test test test

### Features that I may add:

- It would be nice to be able to dowlnoad historic data in various of file types (e.g. pdg, excel, csv, html, etc)
- Configurable via page (something like mini admin panel, at least for urls to be configurable during runtime)
- Setup build on jenkins
- Migrate to aws, and implement with usage of aws services
- Add tests
- handle more requests/endpoints from openweather api
- least important but would be nice to redo frontend (tho I lack in that area)
- in weather prediction will be good if there would be an option to get user location (by ip I guess, microservice for that?)
- probably not as a microservice but as a another lib: redis cache

## Setup (writing setup in progress):

1. Get api key from: [openweathermap.org](https://openweathermap.org/) (getting this key may take up to few hours)
2. Create user secrets in WeatherService project: `dotnet user-secrets init`
   <br /> There should be this entry in WeatherService.csproj:
   <br /> `<UserSecretsId>some_guid</UserSecretsId>`
3. You can look up all your stored secrets with this command: `dotnet user-secrets list`
4. Set up your api key secret in WeatherService project: `dotnet user-secrets set "ServiceSettings:ApiKey" "your_api_key_here"`

Setup Redis on Windows 10:
[Running Redis on Windows 10](https://redislabs.com/blog/redis-on-windows-10/)

1. Get WSL up and running: [How to setup WSL](https://docs.microsoft.com/en-us/windows/wsl/install-win10) (chose any Linux distribution you like, I use Ubuntu)
2. Install redis: `sudo apt-get install redis-server`
3. Restart just to make sure its running: `sudo service redis-server restart`
4. Check version: `redis-cli -v`
5. Start, stop: `sudo service redis-server start` `sudo service redis-server stop`
6. To use redis (to verify Redis is running): `redis-cli`
