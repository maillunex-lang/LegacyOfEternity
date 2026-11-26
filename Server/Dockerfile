FROM golang:1.23-alpine AS builder
WORKDIR /src
COPY go.mod ./
COPY cmd ./cmd
RUN CGO_ENABLED=0 GOOS=linux go build -trimpath -ldflags="-s -w" -o /out/loe-server ./cmd/server

FROM alpine:3.21
RUN adduser -D -H -u 10001 loe
USER loe
COPY --from=builder /out/loe-server /usr/local/bin/loe-server
EXPOSE 18780
ENV LOE_HTTP_ADDR=:18780
ENTRYPOINT ["/usr/local/bin/loe-server"]
