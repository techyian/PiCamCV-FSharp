namespace PiCamCV.Capture

    open Emgu.CV
    open Emgu.CV.CvEnum
    open Emgu.Util
    open System
    open PiCamCV.Capture.Interfaces
   
    (* The CaptureEmgu class is used to call upon the individual features provided by EmguCV. Many of the methods
    in here are called directly by PInvoke to OpenCV itself. *)
    [<AbstractClass>]
    type CaptureEmgu(cap: Capture) = 
        inherit DisposableObject()

        let capture = cap

        let imageGrabbed = new Event<EventHandler, EventArgs>()

        member val Capture = capture with get, set

        member this.Retrieve(outputArray : IOutputArray) = capture.Retrieve(outputArray)

        interface ICaptureGrab with

            member this.Start() = capture.Start()

            member this.Stop() = capture.Stop()

            member this.Pause() = capture.Pause()

            member this.Retrieve(image: IOutputArray) = this.Retrieve(image)

            member this.GetCaptureProperty(index: CapProp) = capture.GetCaptureProperty(index)

            member this.SetCaptureProperty(property: CapProp, value: double) = 
                capture.SetCaptureProperty(property, value)

            member this.FlipHorizontal with get () : bool = this.Capture.FlipHorizontal
                                                                        and set (value) = 
                                                                            match value with 
                                                                            | true -> this.Capture.FlipHorizontal <- true 
                                                                            | _ -> this.Capture.FlipHorizontal <- false
            member this.FlipVertical with get () : bool = this.Capture.FlipVertical
                                                                        and set (value) = 
                                                                            match value with 
                                                                            | true -> this.Capture.FlipVertical <- true
                                                                            | _ -> this.Capture.FlipVertical <- false

            member this.QueryFrame() = capture.QueryFrame()

            member this.QuerySmallFrame() = capture.QuerySmallFrame()
        
        override this.DisposeObject() = capture.Dispose()

    type CaptureFile(filename: string) = 
     inherit CaptureEmgu(new Capture(filename))
