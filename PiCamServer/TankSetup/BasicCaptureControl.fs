namespace PiCamCV.TankSetup          
    open System
    open Emgu.CV   
    open Emgu.CV.CvEnum
    open Emgu.CV.Structure
    open PiCamCV.Capture.Interfaces
            
    type BasicCaptureControl(capture: ICaptureGrab) = 
        inherit BaseCameraConsumer(capture)
                               
        new(capture, camColour) = BasicCaptureControl(capture)

        override x.ImageGrabbedHandler with get (cameraColour: CameraColour, width: double, height: double) : byte[] =
                                        use mat = new Mat()
                                        capture.SetCaptureProperty(CapProp.FrameWidth, width) |> ignore
                                        capture.SetCaptureProperty(CapProp.FrameHeight, height) |> ignore
                                        //Console.WriteLine("DEBUG: Attempt to retrieve frame")
                                        let ret = capture.Retrieve(mat)
                                        
                                        match ret with
                                        | true ->                                                
                                                let captured = mat.ToImage<Bgra, byte>()
                                                match cameraColour with
                                                | CameraColour.Gray -> 
                                                            let grayScale = captured.Convert<Gray, Byte>().ToJpegData(90)
                                                            grayScale
                                                | CameraColour.Bgr -> 
                                                            let bgrScale = captured.Convert<Bgr, Byte>().ToJpegData(90)
                                                            bgrScale
                                                | CameraColour.Bgra -> 
                                                            let bgraScale = captured.Convert<Bgra, Byte>().ToJpegData(90)
                                                            bgraScale
                                                | CameraColour.Hsv -> 
                                                            let hsvScale = captured.Convert<Hsv, Byte>().ToJpegData(90)
                                                            hsvScale
                                                | CameraColour.Hls -> 
                                                            let hlsScale = captured.Convert<Hls, Byte>().ToJpegData(90)
                                                            hlsScale
                                                | CameraColour.Lab -> 
                                                            let labScale = captured.Convert<Lab, Byte>().ToJpegData(90)
                                                            labScale
                                                | CameraColour.Luv -> 
                                                            let luvScale = captured.Convert<Luv, Byte>().ToJpegData(90)
                                                            luvScale
                                                | CameraColour.Xyz -> 
                                                            let xyzScale = captured.Convert<Xyz, Byte>().ToJpegData(90)
                                                            xyzScale
                                                | CameraColour.Ycc -> 
                                                            let yccScale = captured.Convert<Ycc, Byte>().ToJpegData(90)
                                                            yccScale
                                                | _ -> 
                                                            let normalScale = captured.ToJpegData(90)
                                                            normalScale

                                        | false -> 
                                                Console.WriteLine("DEBUG: Retrieve returned false. GrabState assumed not running.")
                                                Array.empty<byte>
                                                                             
                                        
                                           