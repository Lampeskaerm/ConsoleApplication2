module InitView

open System
open System.Net
open System.Threading 
open System.Windows.Forms
open System.Drawing


//Initialize forms and window
let gameWindow =
    new Form(Text="Nim", Size=Size(600,600))

let startWindow =
    new Form(Text="Nim", Size=Size(600,600))

let playerHeader = 
    new Label (Location=Point(100,50), MinimumSize=Size(200,50))

let heaps = 
    new Label(Location=Point(50,100),MinimumSize=Size(500,25),
                MaximumSize=Size(500,50))

let heapChoiceBox =
    new TextBox(Location=Point(150,300),Size=Size(100,25))

let numberOfMatchesBox = 
    new TextBox(Location=Point(350,300),Size=Size(100,25))

let label1 =
    new Label(Location=Point(150,270),Text="Write heap number")

let label2 =
    new Label(Location=Point(350,270),Text="No. of matches")

let provoLabel =
    new Label(Location=Point(200,220),Size=Size(200,25))

let submitBtn =
    new Button(Location=Point(250,400),MinimumSize=Size(100,25),
                MaximumSize=Size(100,50),Text="Submit")

let restartBtn =
    new Button(Location=Point(400,400),MinimumSize=Size(100,25),
                Text="Restart")

let AIChoice = 
    new CheckBox(Location=Point(250,100))

let AIChoiceLabel =
    new Label(Location=Point(150,100), Text="Play only against AI")

let noOfPlayers =
    new TextBox(Location=Point(250,300),Size=Size(100,25), Text="1")

let noOfPlayersLabel =
    new Label(Location=Point(150,300),Text="No of players if no AI")

let lvlBtn1 =
    new Button(Location=Point(150,150),Size=Size(100,100),Text="Easy")

let lvlBtn2 =
    new Button(Location=Point(275,150),Size=Size(100,100),Text="Normal")

let lvlBtn3 =
    new Button(Location=Point(400,150),Size=Size(100,100),Text="Expert")

let startBtn = 
    new Button(Location=Point(250,400),MinimumSize=Size(100,25),
                MaximumSize=Size(100,50),Text="Start")

let cancelBtn = 
    new Button(Location=Point(350,400),MinimumSize=Size(100,25), Text="Cancel")

let errorBoxMenu =
    new Label (Location=Point(150,500),Size=Size(300,25))

let errorBoxGame = 
    new Label (Location=Point(150,500),Size=Size(300,25))

let initStartWindow =
    startWindow.Controls.Add AIChoice
    startWindow.Controls.Add AIChoiceLabel
    startWindow.Controls.Add noOfPlayers
    startWindow.Controls.Add noOfPlayersLabel
    startWindow.Controls.Add startBtn
    startWindow.Controls.Add cancelBtn
    startWindow.Controls.Add errorBoxMenu
    startWindow.Controls.Add lvlBtn1
    startWindow.Controls.Add lvlBtn2
    startWindow.Controls.Add lvlBtn3

let initGameWindow =
    gameWindow.Controls.Add playerHeader
    gameWindow.Controls.Add submitBtn
    gameWindow.Controls.Add restartBtn
    gameWindow.Controls.Add heapChoiceBox
    gameWindow.Controls.Add numberOfMatchesBox
    gameWindow.Controls.Add label1
    gameWindow.Controls.Add label2
    gameWindow.Controls.Add heaps
    gameWindow.Controls.Add errorBoxGame
    gameWindow.Controls.Add provoLabel

;;