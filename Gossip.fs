module Gossip

open System
open Akka.Actor
open Akka.FSharp

let mutable actorRef : IActorRef list = []

let runGossip (actorRefArr: IActorRef[]) rumor = 
    let jj = (actorRefArr.Length-1)/2
    // fill actorRef with all actors in network for lookups
    for i in 0..actorRefArr.Length-1 do
        actorRef <- actorRef @ [actorRefArr.[i]]

    Console.WriteLine("Sending initial rumor...")
    actorRef.[jj] <! rumor
    

let getRandNum min max =
    let rand = Random()
    rand.Next(min, max)


let gossipSend (neighbors: int[]) rumor = 
    let index = getRandNum 0 neighbors.Length    
    let target = actorRef.[neighbors.[index]]
    target <! rumor


let mygossipActor (neighbors: int[]) (mailbox : Actor<'a>) =    
    let mutable ctr = 0
    let rec loop () = 
        actor {
            let! msg = mailbox.Receive()
            gossipSend neighbors msg
            ctr <- ctr + 1

           
            if ctr < 50 then
                
                if ctr = 1 then
                    mailbox.Context.Parent <! msg
                return! loop()
        }
    loop()