# Sweet Silvia backend on Amazon ECS Express Mode and RDS

The repository-root `Dockerfile` builds the .NET API for Amazon ECR and ECS Express Mode. Local Docker Compose and the local `.env` continue to work independently.

## Local Izipay sandbox

Copy `Sweet_Silvia/.env.local.example` to `Sweet_Silvia/.env`, replace its placeholders with the Izipay **test** Shop ID, REST password and public key, and run the API with Docker Compose. The frontend local `.env` should use `VITE_API_URL=http://localhost:8080` and `VITE_IZIPAY_ENABLED=true`.

Izipay must reach the notification endpoint over public HTTPS, so run an HTTPS tunnel to local port `8080` and use its URL for `Izipay__NotificationUrl`. Update that URL whenever the tunnel changes. Keep the test REST password only in the ignored local `.env`; never use the production password locally.

In `Development`, checkout uses a clearly marked local shipping estimate only when `Olva__ApiKey` is missing. This keeps local Izipay checkout testable without presenting an estimated amount as an official Olva quote. Configure a valid Olva API key to exercise the real quote endpoint; this fallback is disabled outside `Development`.

## AWS architecture

- One ECS Express Mode service for the frontend container.
- One ECS Express Mode service for this backend container.
- One private Amazon RDS for PostgreSQL instance in the same VPC.
- Two private S3 buckets: one for catalog assets and one for payment receipts.
- CloudFront with Origin Access Control in front of only the catalog-assets bucket.
- Secrets Manager for the database connection string, JWT secret, Brevo key and Izipay keys.

Do not expose RDS publicly. Allow inbound TCP `5432` on its security group only from the backend ECS task security group. The frontend never connects directly to RDS.

## Backend image and Express Mode

Build the root `Dockerfile`, push the image to a private ECR repository and create an Express Mode service with:

- Container port: `8080`.
- Health check path: `/health`.
- Minimum and maximum tasks: `1` for the first deployment while `Database__ApplyMigrationsOnStartup=true`.
- Public HTTPS host: initially the generated application URL, then `api.sweetsilviastore.com`.
- Network: the same VPC used by RDS.

ECS Express Mode creates the Fargate service, HTTPS Application Load Balancer, target group, scaling, logging and deployment alarms. Terraform passes the service subnet CIDRs as `ForwardedHeaders:KnownIPNetworks`; do not enable `ASPNETCORE_FORWARDEDHEADERS_ENABLED`, because it trusts forwarded headers from arbitrary sources.

## Configuration

Use `Sweet_Silvia/.env.stable.example` as the complete checklist. Keep ordinary configuration as ECS environment variables and inject these values from Secrets Manager:

- `ConnectionStrings__SweetSilviaDatabase`
- `Jwt__SecretKey`
- `Email__ApiKey`
- `Izipay__ShopId`
- `Izipay__Password`
- `Izipay__HmacSha256Key`
- `Izipay__PublicKey`
- `Olva__ApiKey` when the live courier integration is enabled

Set `Orders__PendingPaymentExpirationHours` (24 by default). A hosted cleanup process cancels only orders still in `PendingReceipt` after this period and releases their stock reservation transactionally. Orders with a receipt already under review are left for the operations team.

For the first deployment, keep `Google__Enabled=false` and `Izipay__Enabled=false` until the production origins and external credentials have been verified. The frontend build must use `VITE_ENABLE_MOCKS=false`, `VITE_IZIPAY_ENABLED=false`, the public API URL in `VITE_API_URL`, and an empty `VITE_GOOGLE_CLIENT_ID` while Google OAuth is disabled. Enable Google only after the final frontend origin has been added to Google Cloud and the same web Client ID is configured in both services.

The Brevo sender must be a verified address. A syntactically valid placeholder is not sufficient for delivery, so configure `Email__From` together with `Email__ApiKey` before testing registration or email verification in production.

Before publishing, run `dotnet test Sweet_Silvia/Sweet_Silvia.sln --configuration Release`. The frontend pipeline must pass `npm run test`, `npm run lint`, `npm run build` and `npm audit --audit-level=high`.

Do not configure `AWS_ACCESS_KEY_ID` or `AWS_SECRET_ACCESS_KEY`. Assign an ECS task role with only these object permissions on the two application buckets:

```text
s3:PutObject
s3:GetObject
s3:DeleteObject
```

Restrict the resource ARNs to `arn:aws:s3:::PUBLIC_BUCKET/*` and `arn:aws:s3:::PRIVATE_BUCKET/*`. Give CloudFront access only to the public bucket. Block all public access on the private receipts bucket; receipts are returned exclusively through authorized API endpoints.

## Database

Create PostgreSQL in RDS with automated backups, deletion protection and storage encryption enabled. Put its complete connection string in Secrets Manager and reference it from the backend service as `ConnectionStrings__SweetSilviaDatabase`.

The production image contains Amazon's global RDS CA bundle at `/app/rds-global-bundle.pem`. Keep `SSL Mode=VerifyFull;Root Certificate=/app/rds-global-bundle.pem` in the RDS connection string so Npgsql validates both encryption and the database hostname.

For the initial release, set `Database__ApplyMigrationsOnStartup=true` and keep one backend task so only one process updates the schema. Before scaling to multiple tasks, run migrations from a separate deployment step or one-off task and set `Database__ApplyMigrationsOnStartup=false` on the service.

## Domains and external services

After both generated ECS URLs pass smoke tests:

1. Add the frontend domain to `Cors__AllowedOrigins__0`.
2. Add the frontend domain to Google OAuth authorized JavaScript origins.
3. Configure `Izipay__NotificationUrl=https://api.sweetsilviastore.com/api/payments/izipay/notifications` in the API and in the Izipay Back Office notification rules.
4. For an end-to-end sandbox test on the public site, put the test REST password, test Shop ID and test public key in Secrets Manager and set `izipay_environment=Sandbox`. Complete Izipay's mandatory scenarios and verify a signed IPN.
5. For live payments, replace those Secrets Manager values with the production Shop ID, REST password and public key; set `izipay_environment=Production`, then explicitly enable the backend and rebuild the frontend with `VITE_IZIPAY_ENABLED=true`. The production notification URL must be `https://api.sweetsilviastore.com/api/payments/izipay/notifications` and configured in Izipay Back Office.

Sandbox and production both use the official REST base URL `https://api.micuentaweb.pe/api-payment`; the account credentials determine the payment mode. The API REST password stays backend-only. The frontend receives only the public key and form token needed by Krypton. Never store the Back Office login password in ECS or Secrets Manager.
