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

module Board = 
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
      | Run of string*string | Start of bool*string | Web of string | Error | Cancelled

    //Disable functions
    let rec disableBtns bs = 
        for b in [InitView.startBtn;InitView.restartBtn;InitView.submitBtn;InitView.lvlBtn1;InitView.lvlBtn2] do 
            b.Enabled  <- true
        for (b:Button) in bs do 
            b.Enabled  <- false

    let rec disableTBs b = function
        | [] -> []
        | (x:TextBox)::xs -> x.Enabled <- b
                             x::disableTBs b xs

    //Run async events
    let ev = AsyncEventQueue()

    let rec turnPlayer(list:int list, p, b, mp) = 
        async {InitView.heaps.Text <- Program.genNewStr list
               InitView.playerHeader.Text <- "Player: " + (string p) 
               ignore (disableTBs true [InitView.heapChoiceBox;InitView.numberOfMatchesBox])
               ignore (disableBtns [])

               let! msg = ev.Receive()
               match msg with
               | Run (h,n) -> let nl = Program.genNewList list (int h,int n)
                              match nl with
                              | [] -> InitView.heaps.Text <- Program.genNewStr nl
                                      return! endScreen("Player " + (string p) + " was the winner!! :D") 
                              | _ ->  match p with
                                      | 1 when b = true -> return! turnAI(nl,"AI", b, mp) 
                                      | _ when p < mp -> return! turnPlayer(nl, p+1, b, mp)
                                      | _ -> return! turnPlayer(list,1,b,mp)
               | Start _ -> InitView.gameWindow.Hide()
                            InitView.startWindow.Show()
                            return! mainMenu()
               | _ -> InitView.errorBoxGame.Text <- "You fucked it up"
                      return! turnPlayer(list,p,b,mp)}

    and turnAI(list:int list, p, b, mp) =
        async{  InitView.heaps.Text <- Program.genNewStr list
                InitView.playerHeader.Text <- p
                ignore (disableTBs false [InitView.heapChoiceBox;InitView.numberOfMatchesBox])
                ignore (disableBtns [InitView.submitBtn;InitView.restartBtn])
           
                do! Async.Sleep(5*1000)

                if AI.getM list <> 0
                then InitView.provoLabel.Text <- "Muahahahahaaaa! You cannot win!"
                let nl = AI.theSmartestAI list
                match nl with
                | [] -> InitView.heaps.Text <- Program.genNewStr nl
                        return! endScreen("The AI was the winner! - Too bad for you. You got beaten by a computer.")
                | _  -> return! turnPlayer(nl,1, b, mp)}

    and endScreen(s) = 
        async{  InitView.playerHeader.Text <- s 
                ignore (disableTBs false [InitView.heapChoiceBox;InitView.numberOfMatchesBox])
                ignore (disableBtns [InitView.submitBtn])
            
            
                let! msg = ev.Receive()
                match msg with
                | Start _ -> InitView.gameWindow.Hide()
                             InitView.startWindow.Show()
                             return! mainMenu()
                | _ -> InitView.errorBoxGame.Text <- "You did something unknown"
                       return! endScreen(s)}

    and getLevel(b, mp) = 
        async{  InitView.playerHeader.Text <- "Downloading level"
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
        async{ InitView.playerHeader.Text <- "Welcome to Nim!"

               let! msg = ev.Receive()
               match msg with
               | Start (false,"1") -> InitView.errorBoxMenu.Text <- "Trying to get a confidence boost? More than one player please"
                                      return! mainMenu()
               | Start (b,s) ->  if b
                                 then InitView.startWindow.Hide()
                                      InitView.gameWindow.Show()
                                      return! getLevel(b,2)
                                 else  if   (fst(Int32.TryParse(s)))
                                       then InitView.startWindow.Hide()
                                            InitView.gameWindow.Show()
                                            return! getLevel(b,int s)
                                       else InitView.errorBoxMenu.Text <- "The amount of players has to be a number"
                                            return! mainMenu()
               | _ -> InitView.errorBoxMenu.Text <- "Something unexpected happened - can't start the game"
                      return! mainMenu()}

    //Add forms to windows
    InitView.initStartWindow
    InitView.initGameWindow

    //Add functionalities to buttons
    disableBtns [InitView.lvlBtn1;InitView.lvlBtn2]
    InitView.AIChoice.Click.Add (fun _ -> if InitView.AIChoice.Checked then disableBtns [] else disableBtns [InitView.lvlBtn1;InitView.lvlBtn2] )
    InitView.startBtn.Click.Add (fun _ -> ev.Post (Start (InitView.AIChoice.Checked, InitView.noOfPlayers.Text)))
    InitView.submitBtn.Click.Add (fun _ -> ev.Post (Run (InitView.heapChoiceBox.Text,InitView.numberOfMatchesBox.Text)))
    InitView.restartBtn.Click.Add (fun _ -> ev.Post (Start (false,"") ))
;;