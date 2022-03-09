namespace MediaPlayerRPC

open System
open DiscordRPC
open DiscordRPC.Logging
open DiscordRPC.Message
open InfoFetcher
open Windows.Media.Control

module Presence =
    let onReadyEventHandler (_: ReadyMessage) =
        printfn "Ready"
        
    let rec setPresence (client: DiscordRpcClient) =
        let assets = Assets ()
        assets.LargeImageKey <- "icon"
        
        let button = Button ()
        button.Label <- "Watch on YouTube"
        
        let presence = RichPresence ()
        presence.Assets <- assets
        presence.Buttons <- [| button |]
        
        match getTrackInfo () with
        | Yes res ->
            printfn $"{DateTime.Now + res.CurrentTime} , {DateTime.Now + res.EndTime}"
            presence.Timestamps <- Timestamps ( DateTime.UtcNow + res.CurrentTime, (DateTime.UtcNow + res.EndTime))
            presence.Details <- $"{res.Artist} - {res.Title}"
            assets.LargeImageText <- $"{res.Album}"
            button.Url <- $"https://www.youtube.com/results?search_query={res.Artist.Replace (' ', '+')}+{res.Title.Replace (' ', '+')}"
            
            match res.PlaybackStatus with
            | GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing ->
                assets.SmallImageKey <- "play"
                assets.SmallImageText <- "Playing"
            | GlobalSystemMediaTransportControlsSessionPlaybackStatus.Paused ->
                assets.SmallImageKey <- "pause"
                assets.SmallImageText <- "Paused"
            | _ -> ()
            
            client.SetPresence presence
        | No -> client.ClearPresence ()
        
        Async.Sleep 1000 |> Async.RunSynchronously
        setPresence client
    
    let presence () = 
        let client = new DiscordRpcClient "948624514546270208" 
        client.Logger <- ConsoleLogger (LogLevel.Warning, true)
        client.OnReady.Add onReadyEventHandler
        
        match client.Initialize () with
        | true -> printfn "Success"; setPresence client
        | false -> printfn "Failed to start"
