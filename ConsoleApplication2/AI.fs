//The AI
module AI

type Heap = int
type Level = Heap List


module AI =
    //Calculates m
    //getM: Level -> Heap
    let getM (lvl:Level) =
        match lvl with
        | [] -> failwith("No heaps?")
        | a::la -> a ^^^ List.fold(fun acc x -> x^^^acc) 0 la

    //finds the k where ak xorb m is smaller than ak
    //findK: Heap -> Level -> Level
    let rec findK (m:Heap) (lvl:Level) :Level =
        match lvl with
        | [] -> []
        | a::la ->  let test = if (a^^^m) < a then true else false
                    if test 
                    then (if (a^^^m) = 0 then la else (a^^^m)::la) else a::findK m la

    //Returns the size of the largest heap in the list
    //getLargestHeap: Heap -> Level -> Heap
    let rec getLargestHeap (n:Heap) (lvl:Level) = 
        match lvl with
        | [] -> n
        | a::la -> if a > n then getLargestHeap a la else getLargestHeap n la

    //Takes one from the biggest heap
    //replaceA: Level -> Level
    let rec replaceA list :Level =
        let size = getLargestHeap 0 list 
        match list with
        | [] -> failwith("This shouldn't happen")
        | a::la -> if a = size then (a-1)::la else a::replaceA la

    //The most clever AI that can be made
    //theSmartestAI: Level -> Level
    let theSmartestAI (lvl:Level) = 
        match lvl with
        | [] -> failwith ("No more heaps")
        | la -> let m = getM la
                if m <> 0 then findK m la else replaceA la

    //Tests
    //let getM1 = getM [109;43];;
    //let getM2 = getM [109;43;80];;

    //let replaceA1 = replaceA [109;43];;

    //let findK1 = findK getM1 [109;43];;
    //let check1 = getM findK1;;

    //let theSmartestAI1 = theSmartestAI [109;43];;
    //let theSmartestAI2 = theSmartestAI theSmartestAI1;;

;;
