namespace PiCamCV.Capture
     
    open System
    open System.IO
    open PiCamCV    
    open Common.Logging
    open PiCamCV.Capture.Interfaces

    type CaptureDevice = Unknown | Usb | Pi
    
    type CaptureRequest() = 
        [<DefaultValue>] val mutable File : FileInfo    
        member val Device = CaptureDevice.Usb with get, set
        member val CameraIndex = 0 with get, set        
        member val Config = new CaptureConfig(new Resolution(300, 300), 0, 0, false) with get, set
        
    [<AbstractClass; Sealed>]
    type CaptureFactory() =
        static let log = LogManager.GetCurrentClassLogger()
        
        static member EmitFileWarnings(request: CaptureRequest) = 
            if(not request.File.Exists) then log.Error(fun (m : FormatMessageHandler) -> "Video file '{0}' not found", request.File.FullName)
                                
        static member GetCapture(request : CaptureRequest) : ICaptureGrab= 
            log.Info(fun m -> ("", String.Empty))
            
            match request.File with
            | null -> if(request.Device = CaptureDevice.Pi) then 
                          let cap = new CapturePi(request.Config) :>  ICaptureGrab                                                    
                          cap
                      else
                          let usbCapture = new CaptureUsb(request.CameraIndex) :>  ICaptureGrab                  
                          usbCapture
            | _    -> let cap = new CaptureFile(request.File.FullName) :>  ICaptureGrab
                      cap
         
                


    
    

