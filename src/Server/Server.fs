open System.IO
open Saturn
open Giraffe

let tryGetEnv = System.Environment.GetEnvironmentVariable >> function null | "" -> None | x -> Some x

let publicPath = Path.GetFullPath "../Client/public"

let port = "PORT" |> tryGetEnv |> Option.map uint16 |> Option.defaultValue 5000us

let webApp = router {
    get "/about" (text "Made by Sebastian")
}

let app = application {
    url ("http://0.0.0.0:" + port.ToString() + "/")
    memory_cache
    use_router webApp
    use_static publicPath
    use_json_serializer(Thoth.Json.Giraffe.ThothSerializer())
    use_gzip
}

run app
