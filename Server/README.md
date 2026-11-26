# MMO backend (scaffold)

Go HTTP server: `GET /health` only. No accounts, persistence, world tick, authoritative movement, chat, or combat yet.

```bash
cd Server
go test ./...
go run ./cmd/server
```

Environment: `LOE_HTTP_ADDR=127.0.0.1:18780` (default). Docker Compose uses `:18780` inside the container and maps only to loopback on the VPS. Database is an independent PostgreSQL 16 container provisioned for future schema. Do not expose raw DB ports on the internet.
