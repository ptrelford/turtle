﻿(*[omit:Skip namespace definition on TryFSharp.org]*)
#if INTERACTIVE
open Microsoft.TryFSharp
#else
namespace Turtle
#endif
(*[/omit]*)

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Media
open System.Windows.Shapes
    
[<AutoOpen>]
module AST =    
    type reference = string
    type distance = int
    type degrees = int
    type instruction =
        | Forward of distance
        | Left of degrees
        | Right of degrees
        | Repeat of int * instruction list    
        | PenDown 
        | PenUp
        | PenColor of Color
        | PenWidth of distance
        | ClearScreen
        | ShowTurtle
        | HideTurtle
    
[<AutoOpen>]
module Lang =
    let forward n = Forward n
    let fd = forward
    let left n = Left n
    let lt = left
    let right n = Right n
    let rt = right
    let repeat n (instructions:instruction list) =
        Repeat(n,instructions)
    let once (instructions:instruction list) =
        Repeat(1,instructions)
    let run = once
    let output x = x
    let pencolor color = PenColor color
    let penup = PenUp
    let pendown = PenDown
    let penwidth n = PenWidth n
    let clearscreen = ClearScreen
    let cs = clearscreen
    let hideturtle = HideTurtle
    let ht = hideturtle
    let showturtle = ShowTurtle
    let st = showturtle
   
[<AutoOpen>]
module Colors =
    let red = Colors.Red
    let green = Colors.Green
    let blue = Colors.Blue
    let white = Colors.White
    let black = Colors.Black
    let gray = Colors.Gray
    let yellow = Colors.Yellow
    let orange = Colors.Orange
    let brown = Colors.Brown
    let cyan = Colors.Cyan
    let magenta = Colors.Magenta
    let purple = Colors.Purple

type Turtle private () =
    inherit UserControl(Width = 400.0, Height = 300.0)    
    let screen = Canvas(Background = SolidColorBrush Colors.Black)
    do  base.Content <- screen
    let mutable penColor = white
    let mutable penWidth = 1.0
    let mutable isPenDown = true
    let mutable x, y = 0.0, 0.0
    let mutable a = 270
    let addLine (canvas:Canvas) x' y' =
        let line = Line(X1=x,Y1=y,X2=x',Y2=y')
        line.StrokeThickness <- penWidth
        line.Stroke <- SolidColorBrush penColor 
        canvas.Children.Add line
    let clearLines () = screen.Children.Clear()
    let turtle = Canvas()
    let rotation = RotateTransform(Angle=float a)
    do  turtle.RenderTransform <- rotation    
    do  screen.Children.Add turtle
    let rec execute canvas = function
        | Forward n ->
            let n = float n
            let r = float a * Math.PI / 180.0
            let dx, dy = float n * cos(r), float n * sin(r)
            let x', y' = x + dx, y + dy
            if isPenDown then addLine canvas x' y'
            x <- x'
            y <- y'
        | Left n -> 
            a <- a - n
            rotation.Angle <- float a
        | Right n -> 
            a <- a + n
            rotation.Angle <- float a
        | Repeat(n,instructions) ->
            for i = 1 to n do
                instructions |> List.iter (execute canvas)
        | PenDown -> isPenDown <- true
        | PenUp -> isPenDown <- false
        | PenColor color -> penColor <- color
        | PenWidth width -> penWidth <- float width
        | ClearScreen -> clearLines ()
        | ShowTurtle ->
            turtle.Visibility <- Visibility.Visible
        | HideTurtle ->
            turtle.Visibility <- Visibility.Collapsed
    let drawTurtle () =
        [penup; forward 5; pendown; 
         right 150;  forward 10; 
         right 120; forward 10; 
         right 120; forward 10; 
         right 150; penup; forward 5; right 180; pendown]
        |> List.iter (execute turtle)    
    do  drawTurtle ()
        x <- base.Width/2.0
        y <- base.Height/2.0
    do  Canvas.SetLeft(turtle,x)
        Canvas.SetTop(turtle,y)
    static let control = lazy(Turtle ())    
    member private this.Execute instruction = 
        execute screen instruction
    /// Turtle screen
    static member Screen = control.Force()    
    /// Runs turtle instruction
    static member Run (instruction:instruction) =
        let run () = control.Force().Execute instruction
#if INTERACTIVE
        App.Dispatch(fun () -> run ()) |> ignore
#else
        run ()
#endif
    /// Runs sequence of turtle instructions
    static member Run (instructions:instruction seq) =
        let run () = instructions |> Seq.iter (control.Force().Execute)
#if INTERACTIVE
        App.Dispatch(fun () -> run ()) |> ignore
#else
        run ()
#endif
        
    
(*[omit:Run script on TryFSharp.org]*)
#if INTERACTIVE
App.Dispatch (fun() -> 
    App.Console.ClearCanvas()
    let canvas = App.Console.Canvas
    let control = Turtle.Screen
    control |> canvas.Children.Add
    App.Console.CanvasPosition <- CanvasPosition.Right
    control.Focus() |> ignore
)
#else
type App() as app =
  inherit System.Windows.Application()  
  do  app.Startup.Add(fun _ -> app.RootVisual <- Turtle.Screen)
#endif
(*[/omit]*)

module Test =
   
    do  pencolor red
        |> Turtle.Run 
    
    do  repeat 10 [rt 36; repeat 5 [fd 54; rt 72]]
        |> Turtle.Run
    
    //do  [for x = 0 to 99  do yield once [fd (x * 3); rt 121]]
    //    |> Turtle.Run
      
    //do  [for a = 0 to 1000 do yield once [fd 3; rt (a *7)]] 
    //    |> Turtle.Run

    //  REPEAT 1000[FD 3; RT ANGLE; ANGLE <- ANGLE + 7] 
    //do  [0..1000] |> List.collect (fun a -> [fd 3; rt (a * 7)])
    //    |> Turtle.Run

    //do  [for a = 0 to 1000 do yield! [fd 6; rt (a*7)]]
    //    |> Turtle.Run

    do  hideturtle |> Turtle.Run