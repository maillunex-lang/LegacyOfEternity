# VPS deployment (Ubuntu)

Run from repository root:
```bash
cp Deploy/.env.example Deploy/.env
# Set a strong unique LOE_POSTGRES_PASSWORD value inside Deploy/.env.
docker compose --env-file Deploy/.env -f Deploy/docker-compose.yml config
docker compose --env-file Deploy/.env -f Deploy/docker-compose.yml up -d --build
curl --fail http://127.0.0.1:18780/health
```
Server bound to **127.0.0.1**. PostgreSQL has **no published port**. The Go health service is a technical scaffold and does not implement MMO gameplay. For public access add HTTPS/reverse proxy, authentication, rate limiting, monitoring and backups before exposing routes. Keep the existing VPS applications untouched and change `LOE_BIND_PORT` if 18780 is busy.
