module Board

open Program

open System
open System.Net
open System.Threading 
open System.Windows.Forms
open System.Drawing

//Different functions
// An asynchronous event queue kindly provided by Don Syme 
type AsyncEventQueue<'T>() = 
    let mutable cont = None 
    let queue = System.Collections.Generic.Queue<'T>()
    let tryTrigger() = 
        match queue.Count, cont with 
        | _, None -> ()
        | 0, _ -> ()
        | _, Some d -> 
            cont <- None
            d (queue.Dequeue())

    let tryListen(d) = 
        if cont.IsSome then invalidOp "multicast not allowed"
        cont <- Some d
        tryTrigger()

    member x.Post msg = queue.Enqueue msg; tryTrigger()
    member x.Receive() = 
        Async.FromContinuations (fun (cont,econt,ccont) -> 
            tryListen cont);;

//Initialize forms and window
let window =
  new Form(Text="Nim Local Game", Size=Size(600,600));;

let heaps = 
  new Label(Location=Point(50,50),MinimumSize=Size(100,25),
              MaximumSize=Size(100,50));;

let heapChoiceBox =
  new TextBox(Location=Point(150,300),Size=Size(100,25));;

let numberOfMatchesBox = 
    new TextBox(Location=Point(350,300),Size=Size(100,25));;

let label1 =
    new Label(Location=Point(150,270),Text="Write heap number");;

let label2 =
    new Label(Location=Point(350,270),Text="No. of matches");;

let submitBtn =
  new Button(Location=Point(250,400),MinimumSize=Size(100,25),
              MaximumSize=Size(100,50),Text="Submit");;

// An enumeration of the possible events 
type Message =
  | Run of string*string

//Run async events
let ev = AsyncEventQueue();;

let rec newRound(list:int list) = 
    async {heaps.Text <- genNewStr list
           
           let! msg = ev.Receive()
           match msg with
           | Run (h,n) -> let nl = genNewList list (int h,int n)
                          return! newRound(nl)}


//Add forms to window
window.Controls.Add submitBtn;;
window.Controls.Add heapChoiceBox;;
window.Controls.Add numberOfMatchesBox;;
window.Controls.Add label1;;
window.Controls.Add label2;;
window.Controls.Add heaps;;
submitBtn.Click.Add (fun _ -> ev.Post (Run (heapChoiceBox.Text,numberOfMatchesBox.Text)))

Async.StartImmediate (newRound([109;43]));;
window.Show();;