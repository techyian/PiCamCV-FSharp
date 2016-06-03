namespace PiCamCV.Capture
 
    open Emgu.Util
    open Emgu.CV
    open Emgu.CV.CvEnum
    open Emgu.CV.Structure
    open System
    open System.Threading
    open Common.Logging    
    open PiCamCV.Capture.Interfaces

    type CaptureModuleType = 
            Camera | Highgui
     
    type GrabState = 
            Stopped | Running | Pause| Stopping 
           
    type CapturePi(camIndex: int, config: CaptureConfig) as this = 
        inherit UnmanagedObject()
                
        let log = LogManager.GetCurrentClassLogger()

        let mutable ptr = this._ptr

        let mutable grabState = GrabState.Stopped
                
        let imageGrabbed = new Event<EventHandler, EventArgs>()

        let wait(millisecond : int) = Thread.Sleep(millisecond)
                        
        let grab = 
            if(ptr = IntPtr.Zero) then false            
            else 
                imageGrabbed.Trigger(this, EventArgs.Empty)
                true
        
        let run() =             
            try
                try
                    while(grabState = GrabState.Running || grabState = GrabState.Pause) do
                        if(grabState = GrabState.Pause) then
                            wait(100)
                        elif(not grab) then grabState <- GrabState.Stopping
                with
                | ex -> log.Error(ex.Message)
            finally
                grabState <- GrabState.Stopped

                
        let initCapture (camIndex: int, config: CaptureConfig) =           
            if(box config.Res <> null && config.Res.Width = 0) then ptr <- CvInvokeRaspiCamCV.cvCreateCameraCapture(camIndex) else ptr <- CvInvokeRaspiCamCV.cvCreateCameraCapture2(camIndex, config)
            if(ptr = IntPtr.Zero) then raise (NullReferenceException(String.Format("Error: Unable to create capture from camera {0}", camIndex)))
                        
        member val FlipType = FlipType.None with get, set
                      
        member this.Stop() = (this :> ICaptureGrab).Stop()
        member this.Retrieve(outputArray : IOutputArray) = (this :> ICaptureGrab).Retrieve(outputArray)
                        
        interface ICaptureGrab with            
            member this.FlipHorizontal with get () : bool = (this.FlipType &&& Emgu.CV.CvEnum.FlipType.Horizontal) = Emgu.CV.CvEnum.FlipType.Horizontal
                                                                        and set (value) = match value with true -> this.FlipType <- Emgu.CV.CvEnum.FlipType.Horizontal | _ -> this.FlipType <- Emgu.CV.CvEnum.FlipType.None
            member this.FlipVertical with get () : bool = (this.FlipType &&& Emgu.CV.CvEnum.FlipType.Vertical) = Emgu.CV.CvEnum.FlipType.Vertical
                                                                        and set (value) = match value with true -> this.FlipType <- Emgu.CV.CvEnum.FlipType.Vertical | _ -> this.FlipType <- Emgu.CV.CvEnum.FlipType.None

            member this.GetCaptureProperty(index : CapProp) = CvInvokeRaspiCamCV.cvGetCaptureProperty(this.Ptr, camIndex)
            member this.SetCaptureProperty(property : CapProp, value : double) = true
            member this.Start() = 
                match grabState with
                | GrabState.Pause -> grabState <- GrabState.Running
                | GrabState.Stopped | GrabState.Stopping -> 
                    grabState <- GrabState.Running
                    //ThreadPool.QueueUserWorkItem(Run())
                | _ -> ignore()
                            
            member this.Pause() = if(grabState = GrabState.Running) then grabState <- GrabState.Pause
            member this.Stop() = if(grabState <> GrabState.Stopped) then if(grabState <> GrabState.Stopping) then grabState <- GrabState.Stopping            
            member this.Retrieve(outputArray : IOutputArray) = 
                if(this.FlipType = FlipType.None) then                
                    let ptr = CvInvokeRaspiCamCV.cvQueryFrame(this._ptr)
                    use m : Mat = CvInvoke.CvArrToMat(ptr)
                    m.CopyTo(outputArray)                
                    true
                else                
                    use tmp = new Mat()
                    let ptr = CvInvokeRaspiCamCV.cvQueryFrame(this.Ptr)
                    let managedImage = Image<Bgr, Byte>.FromIplImagePtr(ptr)
                    managedImage.Mat.CopyTo(tmp)
                    CvInvoke.Flip(tmp, outputArray, this.FlipType)       
                    true
            member this.QueryFrame() =                                 
                if(grab) then
                    let image = new Mat()
                    let ret = this.Retrieve(image)
                    image
                else
                    null
            member this.QuerySmallFrame() = raise (NotImplementedException("Not yet implemented."))

        new() = new CapturePi(0, new CaptureConfig())
        new(config: CaptureConfig) = new CapturePi(0, config)
        
        member val _captureModuleType : CaptureModuleType = CaptureModuleType.Camera with get, set

        override x.DisposeObject() = 
            log.Info("Releasing capture")
            this.Stop()
            CvInvokeRaspiCamCV.cvReleaseCapture(this._ptr)
            
        
        

