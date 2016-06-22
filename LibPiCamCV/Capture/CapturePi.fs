namespace PiCamCV.Capture
 
    open Emgu.Util
    open Emgu.CV
    open Emgu.CV.CvEnum
    open Emgu.CV.Structure
    open System
    open System.Threading       
    open PiCamCV.Capture.Interfaces
    open System.Threading.Tasks

    type CaptureModuleType = 
            Camera | Highgui
     
    type GrabState = 
            Stopped | Running | Pause| Stopping 
           
    type CapturePi(camIndex: int, config: CaptureConfig) as this = 
        inherit UnmanagedObject()
        
        [<DefaultValue>] 
        val mutable Config : CaptureConfig
         
        //CancellationToken for our long running task 'StartCamera' and is used to signal cancelling of this task via its Cancel method.
        let cameraCancellationToken = new CancellationTokenSource()  
                
        let mutable ptr = this._ptr
                
        let mutable grabState = GrabState.Stopped
         
        let initCapture () =           
            Console.WriteLine("DEBUG: About to INIT.")                  
            match box config.Res with
            | null -> ptr <- CvInvokeRaspiCamCV.cvCreateCameraCapture(camIndex)
            | :? PiCamCV.Capture.Resolution as res -> 
                match res.Width with 
                | 0 -> ptr <- CvInvokeRaspiCamCV.cvCreateCameraCapture(camIndex)
                | _ -> ptr <- CvInvokeRaspiCamCV.cvCreateCameraCapture(camIndex)
            | _ -> failwith "Errored in initCapture"            
 
            if(ptr = IntPtr.Zero) then raise (NullReferenceException(String.Format("Error: Unable to create capture from camera {0}", camIndex)))
                
        do
            this.Config <- config
            initCapture()        
        
                        
        member val FlipType = FlipType.None with get, set
        member val CapturePtr = ptr with get, set

        member this.CaptureConfig with get () = this.Config and set (value) = this.Config <- value
        member this.Stop() = (this :> ICaptureGrab).Stop()        
        member this.Retrieve(outputArray : IOutputArray) = (this :> ICaptureGrab).Retrieve(outputArray)
        
                        
        interface ICaptureGrab with            
            member this.FlipHorizontal with get () : bool = (this.FlipType &&& Emgu.CV.CvEnum.FlipType.Horizontal) = Emgu.CV.CvEnum.FlipType.Horizontal
                                                                        and set (value) = match value with true -> this.FlipType <- Emgu.CV.CvEnum.FlipType.Horizontal | _ -> this.FlipType <- Emgu.CV.CvEnum.FlipType.None
            member this.FlipVertical with get () : bool = (this.FlipType &&& Emgu.CV.CvEnum.FlipType.Vertical) = Emgu.CV.CvEnum.FlipType.Vertical
                                                                        and set (value) = match value with true -> this.FlipType <- Emgu.CV.CvEnum.FlipType.Vertical | _ -> this.FlipType <- Emgu.CV.CvEnum.FlipType.None
            
            member this.GetCaptureProperty(index : CapProp) = CvInvokeRaspiCamCV.cvGetCaptureProperty(this.Ptr, camIndex)
            member this.SetCaptureProperty(property : CapProp, value : double) =                 
                let returned = CvInvokeRaspiCamCV.cvSetCaptureProperty(this.Ptr, (int)property, value)                
                true
            
            member this.Start() = 
                //Console.WriteLine("DEBUG: About to start running")        
                match grabState with
                | GrabState.Pause -> grabState <- GrabState.Running
                | GrabState.Stopped | GrabState.Stopping -> 
                    grabState <- GrabState.Running                                            
                | _ -> ignore()
                                    
            member this.Pause() = if(grabState = GrabState.Running) then grabState <- GrabState.Pause
            member this.Stop() = if(grabState <> GrabState.Stopped) then if(grabState <> GrabState.Stopping) then grabState <- GrabState.Stopping            
            member this.Retrieve(outputArray : IOutputArray) = 
                //Console.WriteLine("DEBUG: Inside retrieve method CapturePi")
                
                match grabState with
                | GrabState.Pause | GrabState.Stopped | GrabState.Stopping -> false
                | GrabState.Running ->                 
                                if(this.FlipType = FlipType.None) then                              
  
                                    let ptr = CvInvokeRaspiCamCV.cvQueryFrame(this.CapturePtr)
                                    use managedImage = Image<Bgr, Byte>.FromIplImagePtr(ptr)                                
                                    managedImage.Mat.CopyTo(outputArray)                    
                                    Console.WriteLine("Disposing managedImage")
                                    managedImage.Dispose()
                                                                        
                                    true
                                else                
                                    use tmp = new Mat()
                                    let ptr = CvInvokeRaspiCamCV.cvQueryFrame(this.CapturePtr)
                                    use managedImage = Image<Bgr, Byte>.FromIplImagePtr(ptr)
                                    managedImage.Mat.CopyTo(tmp)
                                    CvInvoke.Flip(tmp, outputArray, this.FlipType)       
                                    true
            member this.QueryFrame() =                
                if(ptr = IntPtr.Zero) then ignore()                                
                let image = new Mat()
                let ret = this.Retrieve(image)
                image
                
            member this.QuerySmallFrame() = raise (NotImplementedException("Not yet implemented."))

        new() = new CapturePi(0, new CaptureConfig(new Resolution(300, 300), 20, 20, false))
            
        new(config: CaptureConfig) = new CapturePi(0, config)
        
        member val _captureModuleType : CaptureModuleType = CaptureModuleType.Camera with get, set

        override x.DisposeObject() =             
            Console.WriteLine("Releasing capture CapturePi")            
            this.Stop()
            CvInvokeRaspiCamCV.cvReleaseCapture(this._ptr)
            
        
        

