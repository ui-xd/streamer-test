package ice

import (
	"encoding/base64"
	"encoding/json"

	"github.com/pion/webrtc/v3"
)

func DecodeWebRTCConfig(data string) webrtc.Configuration {
	bytes, _ := base64.RawURLEncoding.DecodeString(data)
	result := webrtc.Configuration{}
	json.Unmarshal(bytes, &result)
	return result
}
