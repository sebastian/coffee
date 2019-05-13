module Client

open Elmish
open Elmish.React
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.PowerPack

type GramsOfCoffee = int

type Msg
    = StartBrewing of GramsOfCoffee
    | Tick
    | TickError of exn
    | BrewingDone
    | Reset

type BrewingModel = {
    TicksPerCycle: int
    CurrentTick: int
    GramsOfCoffee: GramsOfCoffee
}

type Model
    = NotStarted
    | Brewing of BrewingModel
    | Finished
    | Broken of string

let init () : Model * Cmd<Msg> =
    // NotStarted, Cmd.none
    Brewing {TicksPerCycle = 100; CurrentTick = 250; GramsOfCoffee = 20}, Cmd.none

let ticksPerSecond = 20
let secondsToTicks seconds = seconds * ticksPerSecond

let numCycles = 6

let currentCycle brewingModel = brewingModel.CurrentTick / brewingModel.TicksPerCycle + 1

let cyclePercentComplete brewingModel =
    let ticksAtEndOfCycle = (currentCycle brewingModel) * brewingModel.TicksPerCycle
    let ticksIntoCycle = brewingModel.TicksPerCycle - ticksAtEndOfCycle + brewingModel.CurrentTick
    (double ticksIntoCycle * 100.0) / double brewingModel.TicksPerCycle

let gramsOfWaterPerGramOfCoffee = 3

let secondsRemaining brewingModel =
    let timePerCycle = brewingModel.TicksPerCycle / ticksPerSecond
    let ticksAtEndOfCycle = (currentCycle brewingModel) * brewingModel.TicksPerCycle
    (ticksAtEndOfCycle - brewingModel.CurrentTick) / ticksPerSecond

let waterPerCycle brewingModel =
    gramsOfWaterPerGramOfCoffee * brewingModel.GramsOfCoffee

let waterLimitForCycle brewingModel =
    waterPerCycle brewingModel * currentCycle brewingModel

let cmdForTick =
    let promiseWithDelay () = promise {
        do! Promise.sleep (1000 / ticksPerSecond)
        return ()
    }
    Cmd.ofPromise promiseWithDelay () (fun () -> Tick) TickError

let update (msg : Msg) (currentModel : Model) : Model * Cmd<Msg> =
    match msg with
    | StartBrewing gramsOfCoffee ->
        let nextModel = Brewing {
            TicksPerCycle = secondsToTicks 45
            CurrentTick = 0
            GramsOfCoffee = gramsOfCoffee
        }
        nextModel, cmdForTick
    | Tick ->
        match currentModel with
        | Brewing state ->
            let updatedState = {state with CurrentTick = state.CurrentTick + 1}
            let nextCommand =
                if currentCycle updatedState > numCycles
                then Cmd.ofMsg BrewingDone
                else cmdForTick
            Brewing updatedState, nextCommand
        | _ -> currentModel, Cmd.none
    | TickError exn ->
        let errorMessage = sprintf "Failing to keep time! This is mysterious! Error is: '%s'" exn.Message
        Broken errorMessage, Cmd.none
    | BrewingDone ->
        Finished, Cmd.none
    | Reset ->
        NotStarted, Cmd.none

let renderWelcome dispatch =
    div [ClassName "start"] [
        h1 [] [str "Start brewing?"]
        p [] [str "How much coffee are you using?"]
        div [ClassName "amount-coffee"] [
            button [OnClick (fun _ -> dispatch <| StartBrewing 15)] [str "15g"]
            button [OnClick (fun _ -> dispatch <| StartBrewing 20)] [str "20g"]
            button [OnClick (fun _ -> dispatch <| StartBrewing 30)] [str "30g"]
            button [OnClick (fun _ -> dispatch <| StartBrewing 40)] [str "40g"]
            button [OnClick (fun _ -> dispatch <| StartBrewing 50)] [str "50g"]
        ]
    ]

let renderFinished dispatch =
    div [ClassName "finished"] [
        h1 [] [str "Enjoy your coffee!"]
        button [OnClick (fun _ -> dispatch Reset)] [str "Restart?"]
    ]

let renderBroken msg dispatch =
    div [ClassName "error"] [
        h1 [] [str "Oh no!"]
        p [] [str msg]
        button [OnClick (fun _ -> dispatch Reset)] [str "Try again?"]
    ]

let renderBrewing brewingState dispatch =
    let currentCycle = currentCycle brewingState
    let renderCycle =
        function
        | i when i < currentCycle ->
            div [ClassName <| "state completed " + (sprintf "state%i" i)] []
        | i when i = currentCycle ->
            div [ClassName <| "state ongoing " + (sprintf "state%i" i)] [
                div [ClassName "progress"; Style [Width (sprintf "%f%%" (cyclePercentComplete brewingState))]] []
            ]
        | i ->
            div [ClassName <| "state pending " + (sprintf "state%i" i)] []

    div [ClassName "brewing"] [
        div [ClassName "info-box"] [
            div [ClassName "time"] [
                str <| sprintf "%is" (secondsRemaining brewingState)
            ]
            div [ClassName "water"] [
                str <| sprintf "Pour %ig water to a total of %ig" (waterPerCycle brewingState) (waterLimitForCycle brewingState)
            ]
            div [ClassName "reset-button"] [
                button [OnClick (fun _ -> dispatch Reset)] [str "Reset"]
            ]
        ]
        div [ClassName "states"]
            ([1 .. numCycles]
            |> List.map renderCycle)
    ]

let view (model : Model) (dispatch : Msg -> unit) =
    match model with
    | NotStarted -> renderWelcome dispatch
    | Finished -> renderFinished dispatch
    | Broken msg -> renderBroken msg dispatch
    | Brewing brewingModel -> renderBrewing brewingModel dispatch

#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

Program.mkProgram init update view
#if DEBUG
|> Program.withConsoleTrace
|> Program.withHMR
#endif
|> Program.withReact "coffee-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run
