namespace PiCamCV.TankSetup          
    open System
    open Emgu.CV   
    open Emgu.CV.CvEnum
    open Emgu.CV.Structure
    open PiCamCV.Capture.Interfaces
    open System.IO.Compression
    open System.Diagnostics
    open Newtonsoft.Json
    open System.IO
    open PiCamCV.Capture
            
    type BasicCaptureControl(capture: ICaptureGrab) = 
        inherit BaseCameraConsumer(capture)
                               
        new(capture, camColour) = BasicCaptureControl(capture)

        override x.ImageGrabbedHandler with get (cameraColour: CameraColour, res: Resolution) : byte[] =
                                        use mat = new Mat()
                                        //capture.SetCaptureProperty(CapProp.FrameWidth, width) |> ignore
                                        //capture.SetCaptureProperty(CapProp.FrameHeight, height) |> ignore
                                        //Console.WriteLine("DEBUG: Attempt to retrieve frame")
                                        let ret = capture.Retrieve(mat)
                                        
                                        Console.WriteLine("I retrieved the image {0}", ret)
                                        match ret with
                                        | true ->                                                
                                                let stopwatch = new Stopwatch()

                                                stopwatch.Start()
                                                
                                                use captured = mat.ToImage<Bgra, byte>()
                                                
                                                use bitmap = 
                                                        match cameraColour with
                                                        | CameraColour.Gray -> 
                                                                    captured.Convert<Gray, Byte>().Bitmap                                                                    
                                                        | CameraColour.Bgr -> 
                                                                    captured.Convert<Bgr, Byte>().Bitmap                                                                    
                                                        | CameraColour.Bgra -> 
                                                                    captured.Convert<Bgra, Byte>().Bitmap                                                                    
                                                        | CameraColour.Hsv -> 
                                                                    captured.Convert<Hsv, Byte>().Bitmap                                                                    
                                                        | CameraColour.Hls -> 
                                                                    captured.Convert<Hls, Byte>().Bitmap                                                                    
                                                        | CameraColour.Lab -> 
                                                                    captured.Convert<Lab, Byte>().Bitmap                                                                    
                                                        | CameraColour.Luv -> 
                                                                    captured.Convert<Luv, Byte>().Bitmap                                                                    
                                                        | CameraColour.Xyz -> 
                                                                    captured.Convert<Xyz, Byte>().Bitmap                                                                    
                                                        | CameraColour.Ycc -> 
                                                                    captured.Convert<Ycc, Byte>().Bitmap                                                                    
                                                        | _ -> 
                                                                    captured.Bitmap
                                                
                                                use memStream = new MemoryStream()
                                                bitmap.Save(memStream, System.Drawing.Imaging.ImageFormat.Jpeg)

                                                stopwatch.Stop()
                                                Console.WriteLine("PERF: Taking image: {0}", stopwatch.Elapsed)
                                                
                                                captured.Dispose() |> ignore
                                                
                                                mat.Dispose() |> ignore

                                                memStream.ToArray()


                                        | false -> 
                                                Console.WriteLine("DEBUG: Retrieve returned false. GrabState assumed not running.")
                                                Array.empty<byte>
                                                
                                                                             
                                        
                                           