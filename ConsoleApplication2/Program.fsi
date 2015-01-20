﻿module Program

type Heap = int
type Level = Heap List
type Index = int
type Amount = int 

module Program =
    val genNewStr : int -> Level -> string
    val genNewList : Level -> Index * Amount -> Level
    val stringToList : string -> Level