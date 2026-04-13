package main

import (
	"encoding/json"
	"fmt"
	"net/http"
)

type TokenReq struct {
	Token string `json:"token"`
}

type ValidationResult struct {
	ID        int    `json:"id"`
	IsServer  bool   `json:"isServer"`
	Recipient string `json:"recipient"`
}

func main() {
	http.HandleFunc("/validate", func(w http.ResponseWriter, r *http.Request) {
		if r.Method != http.MethodPost {
			http.Error(w, "method not allowed", http.StatusMethodNotAllowed)
			return
		}

		var req TokenReq
		if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
			http.Error(w, "bad request", http.StatusBadRequest)
			return
		}

		var result ValidationResult
		switch req.Token {
		case "server":
			result = ValidationResult{ID: 1, IsServer: true, Recipient: "client"}
		case "client":
			result = ValidationResult{ID: 1, IsServer: false, Recipient: "server"}
		default:
			http.Error(w, "unknown token", http.StatusUnauthorized)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		json.NewEncoder(w).Encode(result)
	})

	fmt.Println("Validator service listening on :9090")
	http.ListenAndServe(":9090", nil)
}
