module Board

open Program
open AI

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
let gameWindow =
  new Form(Text="Nim", Size=Size(600,600));;

let startWindow =
  new Form(Text="Nim", Size=Size(600,600));;

let playerHeader = 
    new Label (Location=Point(100,50), MinimumSize=Size(200,50));;

let heaps = 
  new Label(Location=Point(50,100),MinimumSize=Size(100,25),
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

let restartBtn =
  new Button(Location=Point(400,400),MinimumSize=Size(100,25),
             Text="Restart");;

let AIChoice = 
  new CheckBox(Location=Point(250,100));;

let AIChoiceLabel =
  new Label(Location=Point(150,100), Text="Play only against AI");;

let noOfPlayers =
  new TextBox(Location=Point(250,200),Size=Size(100,25));;

let noOfPlayersLabel =
  new Label(Location=Point(150,200),Text="No of players if no AI");;

let won =
  new Label(Location=Point(400,100),Text="Won: 0");;

let lost =
  new Label(Location=Point(400,200),Text="Lost: 0");;

let startBtn = 
   new Button(Location=Point(250,400),MinimumSize=Size(100,25),
              MaximumSize=Size(100,50),Text="Start");;

let errorBoxMenu =
   new Label (Location=Point(150,300),MinimumSize=Size(100,25),
              MaximumSize=Size(200,25));;

// An enumeration of the possible events 
type Message =
  | Run of string*string | Start of bool*string | Web of string | Error | Cancelled

//Disable functions
let disableAI b =     numberOfMatchesBox.Enabled <- b
                      heapChoiceBox.Enabled <- b
                      submitBtn.Enabled <- b;;

let enableRestart b = restartBtn.Enabled <- b;;

//Run async events
let ev = AsyncEventQueue();;

let rec newRound(list:int list, p) = 
    async {heaps.Text <- Program.genNewStr list
           playerHeader.Text <- "Player: " + (string p)

           let! msg = ev.Receive()
           match msg with
           | Run (h,n) -> let nl = Program.genNewList list (int h,int n)
                          match p with
                          | 1 -> return! newRound(nl,2) 
                          | 2 -> return! newRound(nl,1) 
                          | _ -> failwith "Number of players max is 2"
                                  
           | _ -> failwith "You messed it up"}

let rec turnPlayer(list:int list, p, b, mp) = 
    async {heaps.Text <- Program.genNewStr list
           playerHeader.Text <- "Player: " + (string p)
           disableAI true

           let! msg = ev.Receive()
           match msg with
           | Run (h,n) -> let nl = Program.genNewList list (int h,int n)
                          match nl with
                          | [] -> heaps.Text <- Program.genNewStr nl
                                  return! endScreen("Player " + (string p) + " was the winner!! :D") 
                          | _ ->  match p with
                                  | 1 when b = true -> return! turnAI(nl,"AI", b, mp) 
                                  | _ when p < mp -> return! turnPlayer(nl, p+1, b, mp)
                                  | _ -> return! turnPlayer(list,1,b,mp)
                                  
           | _ -> failwith "You messed it up"}

and turnAI(list:int list, p, b, mp) =
    async{  heaps.Text <- Program.genNewStr list
            playerHeader.Text <- p
            disableAI false
           
            do! Async.Sleep(5*1000)

            let nl = AI.theSmartestAI list
            match nl with
            | [] -> heaps.Text <- Program.genNewStr nl
                    return! endScreen("The AI was the winner! - Too bad for you. You got beaten by a computer.")
            | _  -> return! turnPlayer(nl,1, b, mp)}

and endScreen(s) = 
    async{  playerHeader.Text <- s 
            disableAI false
            enableRestart true
            
            
            let! msg = ev.Receive()
            match msg with
            | Start _ -> gameWindow.Hide()
                         startWindow.Show()
                         return! mainMenu()
            | _ -> failwith "You did something unknown"}

and getLevel(b, mp) = 
    async{  playerHeader.Text <- "Downloading level"
            use ts = new CancellationTokenSource()

            Async.StartWithContinuations
             (async { let webCl = new WebClient()
                      let! html = webCl.AsyncDownloadString(Uri "http://www2.compute.dtu.dk/~mire/nim.game")
                      return html },
              (fun html -> ev.Post (Web html)),
              (fun _ -> ev.Post Error),
              (fun _ -> ev.Post Cancelled),
              ts.Token)
              
            let! msg = ev.Receive()
            match msg with
            | Web html -> let list = Program.stringToList html
                          return! turnPlayer (list,1,b,mp)
            | _ -> failwith "returned error or was cancelled"}

and mainMenu() =
    async{ playerHeader.Text <- "Welcome to Nim!"

           let! msg = ev.Receive()
           match msg with
           | Start (b,s) ->  let check = fst (Int32.TryParse(s))
                             if check 
                             then startWindow.Hide()
                                  gameWindow.Show()
                                  enableRestart false
                                  return! getLevel(b,int s)
                             else errorBoxMenu.Text <- "The amount of players has to be a number"
           | _ -> errorBoxMenu.Text <- "Something unexpected happened - can't start the game"}

//Add forms to windows
startWindow.Controls.Add AIChoice
startWindow.Controls.Add AIChoiceLabel
startWindow.Controls.Add noOfPlayers
startWindow.Controls.Add noOfPlayersLabel
startWindow.Controls.Add startBtn
startWindow.Controls.Add won
startWindow.Controls.Add lost
startWindow.Controls.Add errorBoxMenu
startBtn.Click.Add (fun _ -> ev.Post (Start (AIChoice.Checked, noOfPlayers.Text)));;

gameWindow.Controls.Add playerHeader;;
gameWindow.Controls.Add submitBtn
gameWindow.Controls.Add restartBtn
gameWindow.Controls.Add heapChoiceBox
gameWindow.Controls.Add numberOfMatchesBox
gameWindow.Controls.Add label1
gameWindow.Controls.Add label2
gameWindow.Controls.Add heaps
submitBtn.Click.Add (fun _ -> ev.Post (Run (heapChoiceBox.Text,numberOfMatchesBox.Text)));;
restartBtn.Click.Add (fun _ -> ev.Post (Start (false,"") ));;
                    

Async.StartImmediate (mainMenu());;
startWindow.Show();;