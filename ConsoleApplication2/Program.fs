module Program

type Heap = int
type Level = Heap List
type Index = int
type Amount = int 

module Program =

    //Generates a string from a list
    //genNewStr: Level -> string
    let rec genNewStr n (lvl:Level) = 
        match lvl with
        | [] -> ""
        | a::(la:int list) -> (string n) + ": " + (string a) + "   " + genNewStr (n+1) la

    //Generates the new list of heaps, when matches are removed
    //If the number of matches to be removed is higher than the number of matches in the heap
    //it will return the same list as it got as input.
    //genNewList: Level -> Index*Amount -> Level
    let rec genNewList (lvl:Level) (h:Index,n:Amount) :Level =
        match (lvl,h,n) with
        | (a::la,h,n) when h = 1 -> if a = n then la else (if a > n then (a-n)::la else a::la)
        | (a::la,h,n) -> a::genNewList la (h-1,n)
        | _ -> failwith "there are not that many heaps"

    //
    //let genRandNumber f t = System.Random().Next(f,t);;
    //
    //let genRandNumberList count f t = 
    //    let rnd = System.Random()
    //    List.init count (fun _ -> rnd.Next(f,t));;
    
    //stringListToInt : string list -> Level
    let rec stringListToInt sl :Level =
        match sl with
        | [] -> []
        | x::xs -> (int x)::stringListToInt xs

    //stringToList : string -> Level
    let stringToList (s:string) =
        let array = s.Split(' ')
        let sl = Array.toList array
        stringListToInt sl

;;