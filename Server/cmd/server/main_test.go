package main

import (
 "net/http"
 "net/http/httptest"
 "strings"
 "testing"
)

func TestHealth(t *testing.T) {
 req:=httptest.NewRequest(http.MethodGet,"/health",nil)
 w:=httptest.NewRecorder()
 newHandler().ServeHTTP(w,req)
 if w.Code!=http.StatusOK {t.Fatalf("unexpected status: %d",w.Code)}
 if !strings.Contains(w.Body.String(),"\"status\":\"ok\""){t.Fatalf("health body: %q",w.Body.String())}
}

func TestNoGameplayRoutes(t *testing.T) {
 req:=httptest.NewRequest(http.MethodGet,"/world",nil)
 w:=httptest.NewRecorder()
 newHandler().ServeHTTP(w,req)
 if w.Code!=http.StatusNotFound {t.Fatalf("unexpected route: %d",w.Code)}
}
