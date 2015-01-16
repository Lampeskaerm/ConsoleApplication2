//The AI

module AI

//Calculates m
//getM: int list -> int
let getM = function
    | [] -> failwith("No heaps?")
    | a::la -> a ^^^ List.fold(fun acc x -> x^^^acc) 0 la;;

//finds the k where ak xorb m is smaller than ak
//findK: int -> int list -> int list
let rec findK m = function
    | [] -> []
    | a::la ->  let test = if (a^^^m) < a then true else false
                if test then (a^^^m)::la else a::findK m la

//Takes one from the first heap
//replaceA: int list -> int list
let replaceA = function
    | [] -> failwith("This shouldn't happen")
    | a::la -> (a-1)::la;;

//The most clever AI that can be made
//theSmartestAI: int list -> int list
let theSmartestAI = function
    | [] -> failwith ("No more heaps")
    | la -> let m = getM la
            if m <> 0 then findK m la else replaceA la;;

//Tests
//let getM1 = getM [109;43];;
//let getM2 = getM [109;43;80];;

//let replaceA1 = replaceA [109;43];;

//let findK1 = findK getM1 [109;43];;
//let check1 = getM findK1;;

//let theSmartestAI1 = theSmartestAI [109;43];;
//let theSmartestAI2 = theSmartestAI theSmartestAI1;;

;;
