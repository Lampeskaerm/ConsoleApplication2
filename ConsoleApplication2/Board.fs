module Board

open Program
open AI
open InitView

open System
open System.Net
open System.Threading 
open System.Windows.Forms
open System.Drawing

type ViewList = View List

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
            tryListen cont)

// An enumeration of the possible events 
type Message =
    | Run of string*string | Start of string | StartAI of int | Web of string | Error | Cancelled | Restart

//Disable functions
let rec disableBtns bs = 
    for b in [InitView.startBtn;InitView.restartBtn;InitView.submitBtn;InitView.lvlBtn1;InitView.lvlBtn2;InitView.lvlBtn3] do 
        b.Enabled  <- true
    for (b:Button) in bs do 
        b.Enabled  <- false

let rec disableTBs b = function
    | [] -> []
    | (x:TextBox)::xs -> x.Enabled <- b
                         x::disableTBs b xs

//Run async events
let ev = AsyncEventQueue();;

let rec turnPlayer(list:int list, p, d, b, mp) = 
    async { InitView.heaps.Text <- Program.genNewStr 1 list
            InitView.playerHeader.Text <- "Player: " + (string p) 
            ignore (disableTBs true [InitView.heapChoiceBox;InitView.numberOfMatchesBox])
            ignore (disableBtns [])

            let! msg = ev.Receive()
            match msg with
            | Run (h,n) -> let nl = Program.genNewList list (int h,int n)
                           match nl with
                           | [] -> InitView.heaps.Text <- Program.genNewStr 1 nl
                                   return! endScreen("Player " + (string p) + " was the winner!! :D") 
                           | _ ->  match p with
                                   | 1 when b = true -> return! turnAI(nl,"AI", d, b, mp) 
                                   | _ when p < mp -> return! turnPlayer(nl, p+1, d, b, mp)
                                   | _ -> return! turnPlayer(list,1,d,b,mp)
            | Restart  -> InitView.gameWindow.Hide()
                          InitView.startWindow.Show()
                          disableBtns [InitView.lvlBtn1;InitView.lvlBtn2;InitView.lvlBtn3]
                          return! mainMenu()
            | _ -> InitView.errorBoxGame.Text <- "You fucked it up"
                   return! turnPlayer(list,p,d,b,mp)}

and turnAI(list:int list, p, d, b, mp) =
    async{  InitView.heaps.Text <- Program.genNewStr 1 list
            InitView.playerHeader.Text <- p
            ignore (disableTBs false [InitView.heapChoiceBox;InitView.numberOfMatchesBox])
            ignore (disableBtns [InitView.submitBtn;InitView.restartBtn])
           
            do! Async.Sleep(5*1000)
            let nl = if d = 3 
                        then AI.expertAI list 
                        else (if d = 2 
                              then AI.normalAI list 
                              else AI.easyAI list) 

            if d = 3
            then if AI.getM list <> 0
                    then InitView.provoLabel.Text <- "Muahahahahaaaa! You cannot win!"

            if nl = [1;1]
            then InitView.provoLabel.Text <- "Muahahahahaaaa! You cannot win!"

            match nl with
            | [] -> InitView.heaps.Text <- Program.genNewStr 1 nl
                    return! endScreen("The AI was the winner! - Too bad for you. You got beaten by a computer.")
            | _  -> return! turnPlayer(nl,1,d, b, mp)}

and endScreen(s) = 
    async{  InitView.playerHeader.Text <- s 
            ignore (disableTBs false [InitView.heapChoiceBox;InitView.numberOfMatchesBox])
            ignore (disableBtns [InitView.submitBtn])
            
            
            let! msg = ev.Receive()
            match msg with
            | Restart _ -> InitView.gameWindow.Hide()
                           InitView.startWindow.Show()
                           disableBtns [InitView.lvlBtn1;InitView.lvlBtn2;InitView.lvlBtn3]
                           return! mainMenu()
            | _ -> InitView.errorBoxGame.Text <- "You did something unknown"
                   return! endScreen(s)}

and getLevel(d, b, mp) = 
    async{  InitView.playerHeader.Text <- "Downloading level"
            disableBtns [InitView.startBtn;InitView.lvlBtn1;InitView.lvlBtn2;InitView.lvlBtn3]
            ignore (disableTBs false [InitView.noOfPlayers])

            do! Async.Sleep(5*1000)

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
                          InitView.startWindow.Hide()
                          InitView.gameWindow.Show()
                          return! turnPlayer (list,1,d,b,mp)
            | Cancelled -> ts.Cancel()
                           disableBtns []
                           return! mainMenu() 
            | _ -> InitView.errorBoxMenu.Text <- "returned error"
                   return! mainMenu() }

and mainMenu() =
    async{ InitView.playerHeader.Text <- "Welcome to Nim!"
           ignore (disableTBs true [])

           let! msg = ev.Receive()
           match msg with
           | Start ("1") -> InitView.errorBoxMenu.Text <- "Trying to get a confidence boost? More than one player please"
                            return! mainMenu()
           | Start (s) ->    if   (fst(Int32.TryParse(s)))
                             then 
                                return! getLevel(0,false,int s)
                             else InitView.errorBoxMenu.Text <- "The amount of players has to be a number"
                                  return! mainMenu()
            | StartAI (d) -> 
                            if d = 1
                            then return! getLevel(1,true,2)
                            else if d = 2
                                    then return! getLevel(2,true,2)
                                    else return! getLevel(3,true,2)
            | Cancelled ->   InitView.errorBoxMenu.Text <- "The process was cancelled"
                             return! mainMenu()
            | _ -> InitView.errorBoxMenu.Text <- "Something unexpected happened - can't start the game"
                   return! mainMenu()}

//Add forms to windows
InitView.initStartWindow
InitView.initGameWindow

//Add functionalities to buttons
disableBtns [InitView.lvlBtn1;InitView.lvlBtn2;InitView.lvlBtn3]
InitView.AIChoice.Click.Add (fun _ -> if InitView.AIChoice.Checked then disableBtns [] else disableBtns [InitView.lvlBtn1;InitView.lvlBtn2;InitView.lvlBtn3] )
InitView.startBtn.Click.Add (fun _ -> ev.Post (Start (InitView.noOfPlayers.Text)))
InitView.submitBtn.Click.Add (fun _ -> ev.Post (Run (InitView.heapChoiceBox.Text,InitView.numberOfMatchesBox.Text)))
InitView.restartBtn.Click.Add (fun _ -> ev.Post (Restart ))
InitView.cancelBtn.Click.Add (fun _ -> ev.Post (Cancelled ))
InitView.lvlBtn1.Click.Add (fun _ -> ev.Post (StartAI (1)))
InitView.lvlBtn2.Click.Add (fun _ -> ev.Post (StartAI (2)))
InitView.lvlBtn3.Click.Add (fun _ -> ev.Post (StartAI (3)))
;;