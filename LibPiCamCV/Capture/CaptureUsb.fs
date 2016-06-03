namespace PiCamCV.Capture
    open Emgu.CV
    open System

    [<Serializable>]
    type CaptureUsb(index: int) = 
        inherit CaptureEmgu(new Capture(index))
                
        new() = new CaptureUsb(0)

    
        
        

