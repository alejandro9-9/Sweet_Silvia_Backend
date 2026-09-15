# Sweet Silvia backend on Amazon ECS Express Mode and RDS

The repository-root `Dockerfile` builds the .NET API for Amazon ECR and ECS Express Mode. Local Docker Compose and the local `.env` continue to work independently.

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
- Minimum and maximum tasks: `1` for the first deployment because migrations currently run during API startup.
- Public HTTPS host: initially the generated application URL, then `api.sweetsilviastore.com`.
- Network: the same VPC used by RDS.

ECS Express Mode creates the Fargate service, HTTPS Application Load Balancer, target group, scaling, logging and deployment alarms. `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true` lets ASP.NET Core honor the ALB scheme headers.

## Configuration

Use `Sweet_Silvia/.env.stable.example` as the complete checklist. Keep ordinary configuration as ECS environment variables and inject these values from Secrets Manager:

- `ConnectionStrings__SweetSilviaDatabase`
- `Jwt__SecretKey`
- `Email__ApiKey`
- `Izipay__ApiKey`
- `Izipay__HashKey`
- `Olva__ApiKey` when the live courier integration is enabled

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

The API applies Entity Framework migrations on startup. Keep one task during the initial release. Before scaling to multiple tasks, move migrations to a separate deployment step or one-off task.

## Domains and external services

After both generated ECS URLs pass smoke tests:

1. Add the frontend domain to `Cors__AllowedOrigins__0`.
2. Add the frontend domain to Google OAuth authorized JavaScript origins.
3. Configure `Izipay__NotificationUrl=https://api.sweetsilviastore.com/api/payments/izipay/notifications`.
4. Configure `Izipay__TermsAndConditionsUrl=https://sweetsilviastore.com/terms-and-conditions`.
5. Complete the full Izipay sandbox flow before setting `Izipay__Enabled=true` and rebuilding the frontend with `VITE_IZIPAY_ENABLED=true`.

The Izipay panel password is not an application secret and must not be stored in ECS or Secrets Manager.
