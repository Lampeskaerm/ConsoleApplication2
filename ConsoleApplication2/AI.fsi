﻿module AI

type Heap = int
type Level = Heap List


val expertAI : Level -> Level
val easyAI : Level -> Level
val normalAI : Level -> Level
val getM : Level -> Heap
