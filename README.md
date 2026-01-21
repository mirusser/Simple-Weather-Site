# Simple Weather Site

### Current state of the project

- Migrated from .NET 5 to .NET 10
- Changed folder structure to be more usable and 'friendly'
- Fixed general errors that prevented project from being fully operational when running on Docker
- Some minor refactor
- Still there is a lot of refactoring to do till I'm gonna be at least pleased with the state of the code
- Added health checks for each app and configured Watchdog
- Added (a little janky but still) OAuth identity server (using duende package for test purposes)

### TODOs:

- general refactor
  - _in progress_
- Implement proper exception handling in each microservice and weather site
  - _in progress_
- In each microservice add proper logging (error, warning, info) using seq I think
  - _in progress_
- Weather site: make proper user interface, add messages/alerts to user on what is going on (alerts on success/failure) and proper visualisation of weather forecast (like proper images and visualisation of data)
- Implement proper healthchecks for each microservice, and implement hangfire job in weather site that will check health of each microservice from time to time, and if any will be unhealthy send mail to me with info about it (add microservice for sending emails?)
  - _in progress_

### Features that I may add:

- Setup build on jenkins
- Add tests
- Migrate to aws, and implement with usage of aws services
- It would be nice to be able to dowlnoad historic data in various of file types (e.g. pdg, excel, csv, html, etc)
- Configurable via page (something like mini admin panel, at least for urls to be configurable during runtime)
- probably not as a microservice but as a another lib: redis cache
- automatic backup of databases
- in weather prediction will be good if there would be an option to get user location (by ip I guess, microservice for that?)
- feature: unod (dunno what I can undo yet, TODO: think about it)
- handle more requests/endpoints from openweather api
- least important but would be nice to redo frontend (tho I lack in that area, maybe use blazor)

### Things to check out:

- **Prometheus and Grafana:** For more complex scenarios, especially when you need more than just health status (e.g., metrics and detailed monitoring), using Prometheus for collecting metrics and Grafana for visualization can be a powerful combination. You would use Prometheus exporters to expose metrics from your services, including health check statuses, and then aggregate and visualize them in Grafana.

- **Consul:** Offers service discovery and health checking capabilities. You can use Consul to keep track of the health of various services in your infrastructure. It requires more setup and infrastructure changes but is powerful for microservices architectures.

## Setup (in progress...):

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
