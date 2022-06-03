module PushSum

open System
open Akka.Actor
open Akka.FSharp


let mutable actorRef : IActorRef list = []


let runPushSum (actorRefArr: IActorRef[]) = 
    let jj = (actorRefArr.Length-1)/2
    // fill actorRef with all actors in network for lookups
    for i in 0..actorRefArr.Length-1 do
        actorRef <- actorRef @ [actorRefArr.[i]]

    Console.WriteLine("Sending initial values...")
    actorRef.[jj] <! (-1.0, -1.0)
    

let getRandNum min max =
    let rand = Random()
    rand.Next(min, max)


let sendPushSum (neighbors: int[]) rumor = 
    let index = getRandNum 0 neighbors.Length    
    let target = actorRef.[neighbors.[index]]
    target <! rumor


let ifConverge (old_value: float * float) (new_value: float * float) = 
    let oldVal = fst(old_value)/snd(old_value)
    let newVal = ((fst(old_value) + fst(new_value))/2.0)/((snd(old_value) + snd(new_value))/2.0)
    abs (oldVal - newVal) < 10.0**(-10.0)


let pushSumActor (value: float) (neighbors: int[]) (mailbox : Actor<float * float>) =    
    let mutable ctr = 0
    let mutable s = value
    let mutable w = 0.0
    let mutable fin = false

    let rec loop () = 
        actor {
            let! msg = mailbox.Receive()
            
            // if actor has converged, pass along value (no calculation)
            if fin then
                sendPushSum neighbors (s, w)
                return! loop()

            // if signal to begin push-sum is recieved, start by sending value to self
            if msg = (-1.0, -1.0) then
                s <- s/2.0
                w <- 0.5
                mailbox.Self <! (s, w)
                return! loop()

            // check convergance
            if ifConverge (s, w) msg then
                    ctr <- ctr + 1
            else
                ctr <- 0
            
            // calculate new sum estimate and send to neighbor
            s <- (s + fst(msg))/2.0
            w <- (w + snd(msg))/2.0
            sendPushSum neighbors (s, w)

            
            if ctr = 3 then
                mailbox.Context.Parent <! (s/w)
                fin <- true
            return! loop()
        }
    loop()