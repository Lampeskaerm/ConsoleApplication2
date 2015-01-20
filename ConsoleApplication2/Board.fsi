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

type AsyncEventQueue<'T> =
    new : unit -> AsyncEventQueue<'T>
    member Post : msg : 'T -> unit
    member Receive : unit -> Async<'T>
    
type Message =
    | Run of string * string
    | Start of string
    | StartAI of int
    | Web of string
    | Error
    | Cancelled
    | Restart

val mainMenu : unit -> Async<'a>
;;