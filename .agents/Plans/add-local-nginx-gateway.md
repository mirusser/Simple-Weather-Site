# Add Local Nginx Gateway

## Summary
- Add nginx to the local Docker app stack so local testing can exercise the same gateway shape as EC2.
- Use a separate local nginx config, per preference, and bind it to `http://localhost:8080`.
- Keep direct service ports like `8084` and `8897` for debugging, but document `8080` as the preferred full-site gateway URL.

## Key Changes
- Add `src/deploy/nginx.local.conf` with local gateway routes:
  - `/` proxies to `http://weathersite:80`;
  - `/signalr/` proxies to `http://signalrserver:80`;
  - preserve WebSocket upgrade headers and long SignalR timeouts.
- Update `src/deploy/docker-compose.local.yml`:
  - add a default `gateway` service using `nginx:alpine`;
  - bind `8080:80`;
  - mount `./deploy/nginx.local.conf:/etc/nginx/conf.d/default.conf:ro`;
  - depend on `weathersite` and `signalrserver`;
  - add `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true` to local `weathersite` and `signalrserver` for proxy parity with prod.
- Update docs:
  - `src/deploy/README.md`: list local gateway as `http://localhost:8080` and call it the preferred full-site test URL.
  - `AdditionalInfo/Observability.md`: add Gateway to local service URLs and keep direct service URLs as debug endpoints.

## Test Plan
- Static/config checks:
  - `docker compose --project-directory src -f src/deploy/docker-compose.local.yml config`
  - verify rendered local config contains `gateway`, `8080:80`, and the `nginx.local.conf` mount.
  - `git diff --check`
- Runtime checks when Docker/app images are available:
  - run local app stack;
  - `curl -i http://localhost:8080/health`;
  - open `http://localhost:8080`;
  - verify SignalR connects through `/signalr/WeatherHistoryHub`;
  - confirm direct `http://localhost:8084` still reaches WeatherSite for debugging.

## Assumptions
- Local gateway should start by default with `docker-compose.local.yml`.
- Local host port `8080` is reserved for nginx gateway testing.
- EC2 `nginx.conf` and prod Compose remain unchanged.
- Direct service ports remain available for local debugging.