# Kubernetes Deployment

This directory adds a Minikube-based Kubernetes deployment path alongside the existing Docker Compose deployment in `src/deploy`.

## Layout

- `base/` contains reusable Kubernetes manifests.
- `overlays/minikube/` contains Minikube-specific config, image tags, and generated config/secret inputs.
- `scripts/` contains helper scripts for building images, deploying, and deleting the local stack.

## Quick Start

1. Review the placeholder values in:
   - `overlays/minikube/app-config.example.env`
   - `overlays/minikube/app-secrets.example.env`
2. Replace the placeholder OpenWeather API key in `overlays/minikube/app-secrets.env` if you want `weatherservice` to become Ready.
3. Run:

   ```bash
   ./src/deploy/k8s/scripts/deploy-minikube.sh
   ```

The deploy script will:

- start the `minikube` profile if needed,
- enable the nginx ingress addon,
- create local `app-config.env` and `app-secrets.env` files from the example files when missing,
- build application images directly inside the Minikube Docker daemon,
- apply the Kubernetes manifests, and
- wait for the main workloads to become ready.

Application images are always built with the fixed local tag `minikube`, which matches the Minikube overlay.
The scripts default to the `minikube` profile. Override with `MINIKUBE_PROFILE=<profile>` if you want a separate profile.
When the deploy script needs to start Minikube itself, it defaults to `6` CPUs and `8192` MB of RAM. Override with `MINIKUBE_CPUS` and `MINIKUBE_MEMORY` if needed.

After deployment, open:

```bash
http://$(minikube -p "${MINIKUBE_PROFILE:-minikube}" ip)/
```

## Common Commands

Build images only:

```bash
./src/deploy/k8s/scripts/build-images-minikube.sh
```

Deploy without rebuilding images:

```bash
SKIP_BUILD=1 ./src/deploy/k8s/scripts/deploy-minikube.sh
```

Delete the Kubernetes deployment:

```bash
./src/deploy/k8s/scripts/delete-minikube.sh
```

Force fresh pods after rebuilding a specific image:

```bash
kubectl rollout restart deployment/<service> -n sws
```

## Notes

- The compose deployment remains unchanged.
- `weatherservice` readiness performs a real OpenWeather request. A placeholder or invalid API key keeps the pod NotReady.
- `iconservice` readiness verifies that the configured icon can actually be returned from Mongo. If seeding failed or the icon collection is empty, the pod stays NotReady.
- The health-check probe defaults are configured in the services themselves:
  - `IconService.ServiceSettings.HealthCheckIcon = "03d"`
  - `WeatherService.ServiceSettings.HealthCheckCityId = 756135`
- Mongo uses a single-node replica set initialized by the `mongo-rs-init` Job.
- The Minikube manifests pin Mongo to `mongo:7.0.30` instead of Mongo 8+, because Mongo 8 was unstable in this local setup on newer Linux kernels.
- Because the overlay reuses the fixed image tag `minikube`, rebuilding images alone does not trigger a Deployment rollout. Use `deploy-minikube.sh` or `kubectl rollout restart ...` after rebuilding.
