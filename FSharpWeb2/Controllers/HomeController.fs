namespace FSharpWeb2.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Web
open System.Web.Mvc
open System.Web.Mvc.Ajax
open System.Net.Http
open System.Web.Http
open System.IO
open System.Threading.Tasks
open System.Threading
open System.Web.Helpers


type HomeController() =
    inherit Controller()
  
    member this.Index () =                       
        this.View()
