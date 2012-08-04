#load "AgentSystem.fs"
open AgentSystem.LAgent
open System
open System.Threading

let tprintfn s = printfn "Executing %s on thread %i" s Thread.CurrentThread.ManagedThreadId
let paralleltprintfn s =
    printfn "Executing %s on thread %i" s Thread.CurrentThread.ManagedThreadId
    Thread.Sleep(300)

let echo = MailboxProcessor<_>.SpawnWorker(tprintfn)
let echos = MailboxProcessor.SpawnParallelWorker(paralleltprintfn, 10)

let messages = ["a";"b";"c";"d";"e";"f";"g";"h";"i";"l";"m";"n";"o";"p";"q";"r";"s";"t"]
printfn "...Just one guy doing the work"
messages |> Seq.iter (fun msg -> echo.Post(msg))
Thread.Sleep 1000
printfn "...With a little help from his friends"
messages |> Seq.iter (fun msg -> echos.Post(msg))