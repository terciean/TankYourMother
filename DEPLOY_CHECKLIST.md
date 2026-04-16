# Deploy Checklist

## Security

- Rotate any credentials that were previously committed.
- Store secrets in environment variables or a secret manager, not tracked files.
- Confirm `ConnectionStrings__DefaultConnection` is set to a real value and not placeholders.
- If using Google login, set both `Authentication__Google__ClientId` and `Authentication__Google__ClientSecret`.
- Ensure production hostnames are set in `AllowedHosts`.

## Pre-deploy validation

- [x] Run `dotnet restore`.
- [x] Run `dotnet build --configuration Release`.
- [x] Run database migration in a controlled deploy step:
  - `dotnet ef database update`
- [x] Verify `/health` responds with HTTP 200 after startup.

## Production runtime

- [ ] Set `ASPNETCORE_ENVIRONMENT=Production`.
- [ ] Run behind HTTPS (reverse proxy or direct TLS).
- [x] Enable structured application logs and central log retention.
- [ ] Configure regular database backups and test restore procedures.

## Post-deploy checks

- [ ] Verify login, report creation, and voting flows.
- [x] Verify admin-only routes are protected.
- [ ] Confirm error monitoring and alerts are active.
- [ ] Capture deployed version and migration ID in release notes.
