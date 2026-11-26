package main

import (
 "encoding/json"
 "log"
 "net/http"
 "os"
 "time"
)

func newHandler() http.Handler {
 mux := http.NewServeMux()
 mux.HandleFunc("GET /health", func(w http.ResponseWriter, r *http.Request) {
  w.Header().Set("Content-Type", "application/json")
  w.Header().Set("Cache-Control", "no-store")
  _ = json.NewEncoder(w).Encode(map[string]string{"service":"legacy-of-eternity","status":"ok","stage":"scaffold"})
 })
 return mux
}

func main() {
 addr := os.Getenv("LOE_HTTP_ADDR")
 if addr == "" {addr = "127.0.0.1:18780"}
 srv := &http.Server{Addr:addr,Handler:newHandler(),ReadHeaderTimeout:5*time.Second}
 log.Printf("legacy-of-eternity health scaffold on %s",addr)
 log.Fatal(srv.ListenAndServe())
}
