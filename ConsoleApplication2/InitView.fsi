module InitView

open System
open System.Net
open System.Threading 
open System.Windows.Forms
open System.Drawing

val gameWindow : Form
val startWindow : Form
val playerHeader : Label
val heaps : Label
val heapChoiceBox : TextBox
val numberOfMatchesBox : TextBox
val label1 : Label
val label2 : Label
val provoLabel : Label
val submitBtn : Button
val restartBtn : Button
val AIChoice : CheckBox
val AIChoiceLabel : Label
val noOfPlayers : TextBox
val noOfPlayersLabel : Label
val lvlBtn1 : Button
val lvlBtn2 : Button
val lvlBtn3 : Button
val startBtn : Button
val cancelBtn : Button
val errorBoxMenu : Label
val errorBoxGame : Label
val initStartWindow : unit
val initGameWindow : unit

;;