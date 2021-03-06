﻿namespace WebSharper.Google.Maps.MarkerClusterer.Tests

open WebSharper

module GoogleMaps =
    open WebSharper.JavaScript
    open WebSharper.Html.Client
    open WebSharper.Google.Maps
    open WebSharper.Google.Maps.MarkerClusterer
    
    [<JavaScript>]
    let Sample buildMap =
        Div [Attr.Style "padding-bottom:20px; width:500px; height:300px;"]
        |>! OnAfterRender (fun mapElement ->
            let center = new LatLng(37.4419, -122.1419)
            let options = new MapOptions(center, 8, MapTypeId = MapTypeId.ROADMAP)
            let map = new Map(mapElement.Body, options)
            buildMap map) 

    /// Sets 30 random markers around the map.
    [<JavaScript>]
    let RandomMarkers() =
        Sample <| fun map ->
            let options = MarkerClustererOptions()
            //options.GridSize <- 60
            //options.MaxZoom <- 10.0
            options.ZoomOnClick <- true
            options.AverageCenter <- true
            options.MinimumClusterSize <- 3

            let clusterer = MarkerClusterer(map,[||],options)
            let guard = ref true
            let addMarkers () =
                // bounds is only available in the "bounds_changed" event.
                let bounds = map.GetBounds()
                let sw = bounds.GetSouthWest()
                let ne = bounds.GetNorthEast()
                let lngSpan = ne.Lng() - sw.Lng()
                let latSpan = ne.Lat() - sw.Lat()
                let rnd = Math.Random
                for i in 1 .. 30 do
                    let point = new LatLng(sw.Lat() + (latSpan * rnd()),
                                           sw.Lng() + (lngSpan * rnd()))
                    let markerOptions = new MarkerOptions(point)
                    markerOptions.Map <- map
                    let marker = new Marker(markerOptions)
                    // add marker to clasterer
                    clusterer.AddMarker(marker)
                    |> ignore
            let tryAddMarkers (_:obj) =
                if !guard then
                    guard := false
                    addMarkers()
                else ()
            Event.AddListener(map, "bounds_changed", tryAddMarkers) |> ignore

    [<JavaScript>]
    let Main ()=
        Div [
            H3 [Text "Clustered Random markers"]
            P [Text "Sets 30 random markers around the map."]
            RandomMarkers ()
    ]


type GoogleMapsViewer() =
    inherit Web.Control()
    [<JavaScript>]
    override this.Body = GoogleMaps.Main () :> _

open WebSharper.Sitelets

type Action = | Index

module Site =

    open WebSharper.Html.Server

    let HomePage ctx =
        Content.Page(
            Title = "WebSharper Google Maps Marker Clusterer Tests",
            Body = [Div [new GoogleMapsViewer()]]
        )

    let Main = Sitelet.Content "/" Index HomePage

[<Sealed>]
type Website() =
    interface IWebsite<Action> with
        member this.Sitelet = Site.Main
        member this.Actions = [Action.Index]

[<assembly: Website(typeof<Website>)>]
do ()
