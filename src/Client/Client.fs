module Client

open Elmish
open Elmish.React

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.PowerPack.Fetch
open Elmish
open Fable.Helpers.React
open Fable.PowerPack

open Shared

type GramsOfCoffee = int

type Msg
    = StartBrewing of GramsOfCoffee
    | Tick
    | TickError of exn
    | BrewingDone
    | Reset

type BrewingModel = {
    NumCycles: int
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
    NotStarted, Cmd.none

let ticksPerSecond = 10
let secondsToTicks seconds = seconds * ticksPerSecond

let currentCycle brewingModel = brewingModel.CurrentTick / brewingModel.TicksPerCycle + 1

let gramsOfWaterPerGramOfCoffee = 3

let secondsRemaining brewingModel =
    let timePerCycle = brewingModel.TicksPerCycle / ticksPerSecond
    timePerCycle - (brewingModel.CurrentTick / currentCycle brewingModel) / ticksPerSecond

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
            NumCycles = 6
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
                if currentCycle updatedState > updatedState.NumCycles
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
            div [ClassName "state completed"] []
        | i when i = currentCycle ->
            div [ClassName "state ongoing"] [
                span [ClassName "time"] [
                    str <| sprintf "%is remaining" (secondsRemaining brewingState)
                ]
                span [ClassName "water"] [
                    str <| sprintf "Pour %ig water to a total of %ig" (waterPerCycle brewingState) (waterLimitForCycle brewingState)
                ]
            ]
        | _i ->
            div [ClassName "state pending"] []

    div [ClassName "brewing"] [
        h1 [] [str "Brew!"]
        div [ClassName "states"]
            ([1 .. brewingState.NumCycles]
            |> List.map renderCycle)
        button [OnClick (fun _ -> dispatch Reset)] [str "Reset"]
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
