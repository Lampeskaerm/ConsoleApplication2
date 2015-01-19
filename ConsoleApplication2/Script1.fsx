#load "Program.fs"
#load "AI.fs"
#load "InitView.fs"
#load "Board.fs"

open Program
open AI
open InitView
open Board

Async.StartImmediate (Board.mainMenu()) 
InitView.startWindow.Show()