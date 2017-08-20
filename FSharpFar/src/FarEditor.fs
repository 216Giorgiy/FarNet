﻿
// FarNet module FSharpFar
// Copyright (c) Roman Kuzmin

namespace FSharpFar

open FarNet
open Checker
open Session
open FsAutoComplete
open Microsoft.FSharp.Compiler.SourceCodeServices
open System

type LineArgs = {
    Text : string
    Index : int
    Column : int
}

type MouseMessage =
| Noop
| Move of LineArgs

[<System.Runtime.InteropServices.Guid "B7916B53-2C17-4086-8F13-5FFCF0D82900">]
[<ModuleEditor (Name = "FSharpFar", Mask = "*.fs;*.fsx;*.fsscript")>]
type FarEditor (editor: IEditor, _args) =
    inherit ModuleEditor ()

    let postExn exn = far.PostJob (fun () -> far.ShowError (exn.GetType().Name, exn))

    let checkAgent = MailboxProcessor.Start (fun inbox -> async {
        while true do
            do! inbox.Receive ()
            if inbox.CurrentQueueLength > 0 then () else

            do! Async.Sleep 2000
            if inbox.CurrentQueueLength > 0 then () else

            editor.PostJob (fun () ->
                let text = editor.GetText ()
                Async.StartWithContinuations (
                    async {
                        let options = editor.getOptions ()
                        let! check = Checker.check editor.FileName text options
                        editor.fsErrors <-
                            if inbox.CurrentQueueLength > 0 then
                                None
                            else
                                let errors = check.CheckResults.Errors
                                if errors.Length = 0 then None else Some errors
                    },
                    (fun () -> editor.PostJob (fun () -> editor.Redraw ())),
                    postExn,
                    ignore
                )
            )
    })

    let mouseAgent = MailboxProcessor.Start (fun inbox -> async {
        while true do
            let! message = inbox.Receive ()
            if inbox.CurrentQueueLength > 0 then () else

            match message with
            | Noop -> ()
            | Move it ->
                do! Async.Sleep 400
                if inbox.CurrentQueueLength > 0 then () else

                let mutable autoTips = editor.fsAutoTips
                match editor.tryMyErrors () with
                | None -> ()
                | Some errors ->
                    let lines =
                        errors
                        |> Array.filter (fun err ->
                            it.Index >= err.StartLineAlternate - 1 &&
                            it.Index <= err.EndLineAlternate - 1 &&
                            (it.Index > err.StartLineAlternate - 1 || it.Column >= err.StartColumn) &&
                            (it.Index < err.EndLineAlternate - 1 || it.Column <= err.EndColumn))
                        |> Array.map strErrorText
                        |> Array.distinct
                    if lines.Length > 0 then
                        autoTips <- false
                        let text = String.Join ("\r", lines)
                        editor.PostJob (fun () -> showText text "Errors")

                if autoTips then
                    match Parsing.findLongIdents (it.Column, it.Text) with
                    | None -> ()
                    | Some (column, idents) ->
                        editor.PostJob (fun () ->
                            let fileText = editor.GetText ()
                            Async.StartWithContinuations (
                                async {
                                    let options = editor.getOptions ()
                                    let! check = Checker.check editor.FileName fileText options
                                    let! tip = check.CheckResults.GetToolTipTextAlternate (it.Index + 1, column + 1, it.Text, idents, FSharpTokenTag.Identifier)
                                    return Checker.strTip tip
                                },
                                (fun tips -> if tips.Length > 0 && inbox.CurrentQueueLength = 0 then editor.PostJob (fun () -> showText tips "Tips")),
                                postExn,
                                ignore
                            )
                        )
    })

    let postNoop _ =  mouseAgent.Post Noop

    do
        if editor.fsSession.IsNone then

            if isSimpleSource editor.FileName then
                editor.fsAutoCheck <- true
                editor.fsAutoTips <- true

            editor.KeyDown.Add <| fun e ->
                match e.Key.VirtualKeyCode with
                | KeyCode.Tab when e.Key.Is () && not editor.SelectionExists ->
                     e.Ignore <- Editor.complete editor
                | _ -> ()

            editor.Changed.Add <| fun e ->
                // We want to keep errors visible, so that after a fixing change we see how they go.
                // This does not work well on massive changes like copy/paste, delete many lines.
                // So lets keep errors only when lines change.
                if e.Kind = EditorChangeKind.LineChanged then
                    editor.fsChecking <- true
                else
                    editor.fsErrors <- None
                if editor.fsAutoCheck then
                    checkAgent.Post ()

            editor.MouseDoubleClick.Add postNoop
            editor.MouseClick.Add postNoop
            editor.MouseWheel.Add postNoop

            editor.MouseMove.Add <| fun e ->
                mouseAgent.Post (
                    if e.Mouse.Is () then
                        let pos = editor.ConvertPointScreenToEditor e.Mouse.Where
                        if pos.Y < editor.Count then
                            let line = editor.[pos.Y]
                            if pos.X < line.Length then
                                Move {Text = line.Text; Index = pos.Y; Column = pos.X}
                            else Noop
                        else Noop
                    else Noop
                )
