# Gossip and Push-Sum Algo Simulator

## Project 2 - DOSP (COP 5615)

`By:  `
`Daanish Goyal`
`UFID: 1767-3302`


This program simulates the asynchronous Gossip and Push-Sum algorithms which is implemented using the actor model of concurrent computation, in F#.


#### Command to run:

`dotnet run <num_nodes> <topology> <algorithm>`



#### Available topologies:

`full` - Every actor is a neighbor of all other          actors.

`line` - Actors are arranged in a line.

`3D` - Actors form a 3D grid.

`imp3D` - Actors are arranged in a grid but one           random other neighbor is selected               from the list of all actors.

#### Available algorithms:

1.`gossip`

2.`push-sum`


### What is working:

* Convergence of `Gossip algorithm` for all topologies - `Full, Line, 3D, Improper 3D`.

* Convergence of `Push Sum algorithm` for all topologies - Full, Line, 3D, Improper 3D.

### Largest Networks:

**Number of Nodes for `Gossip Algorithm`-** 
* Full topology: 20000
* Line topology : 5000
* 3D topology: 10000
* Improper 3D: 10000

**Number of Nodes for `Push Sum Algorithm`-**

* Full topology: 10000
* Line topology: 2000
* 3D topology: 5000
* Improper 3D: 5000