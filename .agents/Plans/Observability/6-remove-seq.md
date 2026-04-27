# Remove Seq And Make Loki/Grafana Primary Logs

## Summary
- Keep Serilog, remove Seq everywhere from runtime infrastructure and app logging config.
- Make Docker stdout JSON logs the canonical app log output.
- Expand Alloy/Loki collection from Cities-only to all .NET app containers.
- Do not delete existing local Seq data automatically; document manual cleanup.

## Key Changes
- Remove Seq from required infra:
  - Delete the `seq` service from Docker infra compose.
  - Remove `/opt/sws/volumes/seq/data` creation from local/EC2 infra scripts.
  - Remove port `5341` from local port-forward/firewall helper scripts.
  - Remove Seq PVC/service/deployment from Kubernetes base manifests and remove the Minikube rollout wait.
- Remove Seq from app logging:
  - Remove every Serilog `Seq` sink from appsettings variants across services.
  - Remove `Serilog.Sinks.Seq` from Cities `Using` arrays and remove the shared `Serilog.Sinks.Seq` package reference from `Common.Presentation`.
  - Keep Serilog, existing enrichers, and the activity trace/span enricher.
  - Ensure container-used configs log to Console as structured JSON; keep Development console output readable unless already Docker-oriented.
- Expand Loki collection:
  - Update Alloy Docker discovery from Cities-only to all .NET app containers: `oauthserver`, `citiesservice`, `citiesgrpcservice`, `weatherservice`, `weatherhistoryservice`, `iconservice`, `signalrserver`, `emailservice`, `hangfireservice`, `backupservice`, `weathersite`.
  - Keep labels low-cardinality: `service_name`, `container`, `level`.
  - Keep `trace_id` and `span_id` as structured metadata/fields, not Loki labels.
  - Exclude `gateway` and infra containers from this pass because they are not Serilog JSON app logs.
- Update docs:
  - Update `AdditionalInfo/Observability.md`, deploy README, and active environment docs to say Loki/Grafana is the primary log backend.
  - Remove Seq startup/check instructions and replace with Grafana Explore/Loki queries.
  - Add stale data notes: existing `/opt/sws/volumes/seq/data` or old Kubernetes Seq PVCs can be manually removed after confirming they are no longer needed.

## Test Plan
- Validate JSON config:
  - Run `jq` per appsettings file, not concatenated, because some files have UTF-8 BOMs.
- Validate builds:
  - `dotnet restore src/SimpleWeather.slnx`
  - `dotnet msbuild src/SimpleWeather.slnx /t:Build /p:Restore=false /p:UseSharedCompilation=false /m:1 /v:minimal`
- Validate tests:
  - `dotnet test src/SimpleWeather.slnx --no-restore /p:UseSharedCompilation=false /m:1 --verbosity minimal`
- Validate infra config:
  - `docker compose config` for infra, prod app, and local app compose files.
  - `kubectl kustomize src/deploy/k8s/overlays/minikube >/dev/null` if `kubectl` is available.
  - Alloy config validation.
- Verify cleanup:
  - `rg` should find no active Seq references in deploy configs, appsettings, project files, or active docs.
  - Runtime smoke: start infra/apps, confirm no `seq_infra` container, confirm Loki receives logs from all app containers, and confirm Cities logs still include `trace_id`/`span_id`.

## Assumptions
- “Full infra purge” means Docker, EC2 helper scripts, Kubernetes manifests, appsettings, package references, and active docs.
- Historical planning notes under `.agents/Plans` do not need cleanup unless they are treated as active docs.
- Serilog stays as the app logging facade; only Seq is removed.
- Existing Seq data is not deleted by scripts to avoid accidental data loss.
