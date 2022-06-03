// Learn more about F# at http://fsharp.org
module Main

open Gossip
open PushSum
open System
open Akka.Actor
open Akka.FSharp
open System.Collections.Generic

let system = ActorSystem.Create("FSharp")

let neighbors = new Dictionary<int, int []>( )

let full_network numNodes = 
    for i in 0..numNodes-1 do
        let mutable adjList : int list = []
        for j in 0..numNodes-1 do
            if i <> j then
                adjList <- j :: adjList
        let adjArray = adjList |> List.toArray
        neighbors.Add(i, adjArray)

let threeD_network numNodes cuberoot =
    let mutable zMax = int(cuberoot)
    let mutable xMax = zMax
    let mutable yMax = zMax
    if ((numNodes - int(Math.Pow(float(zMax), 3.0))) < (int(Math.Pow(float(zMax) + 1.0, 3.0)) - numNodes)) then
        let a = int(Math.Pow(float(zMax),3.0))
        let off = numNodes - a
        zMax <- zMax + int(off/ int(Math.Pow(float(xMax),2.0))) + 1
    else
        xMax <- xMax + 1
        yMax <- yMax + 1
        zMax <- zMax + 1
        let a = int(Math.Pow(float(xMax),3.0))
        let off = numNodes - a
        zMax <- zMax - int(off/ int(Math.Pow(float(xMax),2.0))) - 1
        printfn "%d" zMax

    let mutable i = 0
    let zMulti = int(Math.Pow(float(xMax),2.0))
    let yMulti = xMax
    let xlimit = xMax - 1
    let ylimit = xMax - 1
    let zlimit = xMax - 1

    for z = 0 to zlimit do
        for y = 0 to ylimit do
            for x = 0 to xlimit do
                i <- z*zMulti + y*yMulti + x
                if i < numNodes then
                    let mutable adjList : int list = []
                    // left in same plane
                    if (x > 0) then
                        adjList <- i-1 :: adjList

                    // right in same plane
                    if (x < xlimit && (i + 1) < numNodes) then
                        adjList <- i+1 :: adjList

                    // above in same plane
                    if (y > 0) then
                        adjList <- i-yMulti :: adjList

                    // below in same plane 
                    if (y < ylimit && (i + yMulti) < numNodes) then
                        adjList <- i+yMulti :: adjList

                    // below in diff plane
                    if (z > 0) then
                        adjList <- i-zMulti :: adjList

                    // above in diff plane
                    if (z < zlimit && (i + zMulti) < numNodes) then
                        adjList <- i+zMulti :: adjList

                    let adjArray = adjList |> List.toArray
                    neighbors.Add(i,adjArray)
            


let line_network numNodes = 
    for i in 0..numNodes-1 do
        let mutable adjList : int list = []
        if i = 0 then
            adjList <- i+1 :: adjList
        elif i = numNodes-1 then
            adjList <- i-1 :: adjList
        else
            adjList <- i+1 :: adjList
            adjList <- i-1 :: adjList

        let adjArray = adjList |> List.toArray
        neighbors.Add(i, adjArray)


let impThreeD_network numNodes cuberoot =
    let mutable zMax = int(cuberoot)
    let mutable xMax = zMax
    let mutable yMax = zMax
    if ((numNodes - int(Math.Pow(float(zMax), 3.0))) < (int(Math.Pow(float(zMax) + 1.0, 3.0)) - numNodes)) then
        let a = int(Math.Pow(float(zMax),3.0))
        let off = numNodes - a
        zMax <- zMax + int(off/ int(Math.Pow(float(xMax),2.0))) + 1
    else
        xMax <- xMax + 1
        yMax <- yMax + 1
        zMax <- zMax + 1
        let a = int(Math.Pow(float(xMax),3.0))
        let off = numNodes - a
        zMax <- zMax - int(off/ int(Math.Pow(float(xMax),2.0))) - 1
        printfn "%d" zMax

    let mutable i = 0
    let zMulti = int(Math.Pow(float(xMax),2.0))
    let yMulti = xMax
    let xlimit = xMax - 1
    let ylimit = xMax - 1
    let zlimit = xMax - 1

    for z = 0 to zlimit do
        for y = 0 to ylimit do
            for x = 0 to xlimit do
                i <- z*zMulti + y*yMulti + x
                if i < numNodes then
                    let mutable adjList : int list = []
                    // left in same plane
                    if (x > 0) then
                        adjList <- i-1 :: adjList

                    // right in same plane
                    if (x < xlimit && (i + 1) < numNodes) then
                        adjList <- i+1 :: adjList

                    // above in same plane
                    if (y > 0) then
                        adjList <- i-yMulti :: adjList

                    // below in same plane 
                    if (y < ylimit && (i + yMulti) < numNodes) then
                        adjList <- i+yMulti :: adjList

                    // below in diff plane
                    if (z > 0) then
                        adjList <- i-zMulti :: adjList

                    // above in diff plane
                    if (z < zlimit && (i + zMulti) < numNodes) then
                        adjList <- i+zMulti :: adjList

                    let adjArray = adjList |> List.toArray
                    neighbors.Add(i,adjArray)


let myrefArr (algorithm: string) (numNodes: int) (mailbox : Actor<'a>)=   
    if algorithm = "gossip" then
        [|
            for i in 0 .. numNodes-1 -> 
                (spawn mailbox ("worker"+i.ToString()) (mygossipActor (neighbors.Item(i))))
        |]
    else
        [|
            for i in 0 .. numNodes-1 -> 
                (spawn mailbox ("worker"+i.ToString()) (pushSumActor (float i) (neighbors.Item(i))))
        |]

let activateProtocol (algorithm: string) (refArr: IActorRef []) (numNodes: int) = 
    if algorithm = "gossip" then
        Console.WriteLine("Using Gossip Algorithm...")
        runGossip refArr "rumor"
           
    elif algorithm = "push-sum" then
        Console.WriteLine("Using Push Sum Algorithm...")
        runPushSum refArr

    else    
        Console.WriteLine("Enter either gossip or push-sum")

let supervisorActor (algorithm: string) (numNodes: int) (mailbox : Actor<'a>)= 
    let refArr = myrefArr algorithm numNodes mailbox
    activateProtocol algorithm refArr numNodes
    let mutable heardNum = 0
    let mutable returnAddress = mailbox.Context.Parent
    let rec loop () = 
        actor {
            let! msg = mailbox.Receive()
            let sender = mailbox.Sender()
            if returnAddress = mailbox.Context.Parent then
                returnAddress <- sender
                return! loop()
            else
                heardNum <- heardNum + 1
                if heardNum < numNodes then 
                    return! loop()
                else
                    if algorithm = "gossip" then
                        Console.WriteLine("All {0} nodes heard the rumor!", heardNum)
                    elif algorithm = "push-sum" then
                        Console.WriteLine("All {0} nodes converged to the sum! (~{1})", heardNum, msg)
                    returnAddress <! "done!"
        }
    loop()



let makeTopology topology numNodes = 
    if topology = "full" then
        full_network(numNodes)

    elif topology = "3D" then
        let cuberoot = Math.Cbrt(float(numNodes))
        threeD_network numNodes cuberoot

    elif topology = "line" then
        line_network(numNodes)

    else 
        let cuberoot = Math.Cbrt(float(numNodes)) 
        impThreeD_network numNodes cuberoot

[<EntryPoint>]
let main argv =
    if argv.Length <> 3 then
        Console.WriteLine("Invalid Input Provided")
        Console.WriteLine("Ex.: project2 100 2D gossip")
    else

        let mutable numNodes:int = int argv.[0]
        let topology = argv.[1]
        let algorithm = argv.[2]
  
        makeTopology topology numNodes

        let myWatch = System.Diagnostics.Stopwatch.StartNew()
        let mysupervisor = spawn system "supervisorActor" (supervisorActor algorithm numNodes)
        let res = mysupervisor <? "done?" |> Async.RunSynchronously
        myWatch.Stop()
        Console.WriteLine("Time to complete: {0} ms", myWatch.Elapsed.TotalMilliseconds)

        
    
    0 
