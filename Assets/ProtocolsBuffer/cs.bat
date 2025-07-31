
#protoc --csharp_out=".\Sources" "proto_message.proto"

protoc --csharp_out="..\Sources\Proto" "error_message.proto"
protoc --csharp_out="..\Sources\Proto" "login_message.proto"
protoc --csharp_out="..\Sources\Proto" "room_message.proto"
protoc --csharp_out="..\Sources\Proto" "game_message.proto"
protoc --csharp_out="..\Sources\Proto" "sync_message.proto"