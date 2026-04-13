package signaling

import (
	"encoding/base64"
	"encoding/json"
)

type Signaling struct {
	Wsurl    string `json:"wsurl"`
	Grpcport int    `json:"grpcport"`
	Grpcip   string `json:"grpcip"`
}

func DecodeSignalingConfig(data string) Signaling {
	bytes, _ := base64.RawURLEncoding.DecodeString(data)
	result := Signaling{}
	json.Unmarshal(bytes, &result)
	return result
}
