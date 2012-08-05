#load "..\src\AgentSystem.fs"
open AgentSystem.LAgent

let counter0 =
    MailboxProcessor.Start(fun inbox ->
        let rec loop n =
            async { 
                    let! msg = inbox.Receive()
                    return! loop(n+msg) }
        loop 0)

counter0.Post(3)

let counter1 = MailboxProcessor.SpawnAgent( (fun msg n -> msg + n), 0)

counter1.Post(3)

