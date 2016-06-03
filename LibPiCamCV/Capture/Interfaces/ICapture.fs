namespace PiCamCV.Capture.Interfaces

    open System
    open Emgu.Util
    open Emgu.CV
    open Emgu.CV.CvEnum
    
    (* The ICaptureGrab Interface is used to allow implementation of the individual features supported by EmguCV, such 
    as the rotation of images, and starting/stopping the handle on the Camera itself. *)    
    type ICaptureGrab =     
        inherit ICapture
        inherit IDisposable

        abstract member FlipHorizontal : bool with get, set
        abstract member FlipVertical : bool with get, set
        abstract member Start : unit -> unit
        abstract member Stop : unit -> unit
        abstract member Pause : unit -> unit
        abstract member Retrieve : IOutputArray -> bool
       
        abstract member GetCaptureProperty : CapProp -> double
        abstract member SetCaptureProperty : CapProp*double -> bool





