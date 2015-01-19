module AI

type Heap = int
type Level = Heap List


module AI =
    val expertAI : Level -> Level
    val easyAI : Level -> Level
    val normalAI : Level -> Level
    val getM : Level -> Heap
