module Program

let rec genNewStr = function
    | [] -> ""
    | a::(la:int list) -> (string a) + " " + genNewStr la;;

let rec genNewList la (h,n) =
    match (la,h,n) with
    | (a::la,h,n) -> if h = 1 then (if a = n then la else (a-n)::la) else a::genNewList la (h-1,n)
    | _ -> failwith "Something is definitely wrong here";;