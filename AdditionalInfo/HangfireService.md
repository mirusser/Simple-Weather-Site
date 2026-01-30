# HangfireService

## Manual registration: SQL backup job

Use the HangfireService `RegisterJob` endpoint to create a recurring job that calls the
BackupService `CreateSqlBackup` endpoint.

### Example request (Docker)

```http
POST /api/job/RegisterJob
Content-Type: application/json
```

```json
{
  "JobName": "sql-backup-nightly",
  "ServiceName": "BackupService",
  "CronExpression": "0 2 * * *",
  "JobType": "CallEndpointHttpJob",
  "url": "http://backupservice/api/backup/CreateSqlBackup",
  "HttpMethod": "POST",
  "BodyJson": "{\"backupName\":\"Nightly\"}"
}
```

### Example request (local)

```http
POST /api/job/RegisterJob
Content-Type: application/json
```

```json
{
  "JobName": "sql-backup-nightly",
  "ServiceName": "BackupService",
  "CronExpression": "0 2 * * *",
  "JobType": "CallEndpointHttpJob",
  "Url": "http://localhost:5184/api/backup/CreateSqlBackup",
  "HttpMethod": "POST",
  "BodyJson": "{\"backupName\":\"Nightly\"}"
}
```

### Notes

- `cronExpression` follows standard 5-part cron (min hour day month week).
- The job posts a JSON body `{ "backupName": "Nightly" }` to BackupService.
- `clientName` and `pipelineName` map to configured resilience pipelines.
