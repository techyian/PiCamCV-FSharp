namespace PiCamCV.TankSetup          
    open System
    open Emgu.CV
    open Emgu.CV.CvEnum
    open Emgu.CV.Structure
    open PiCamCV.Capture.Interfaces
            
    type BasicCaptureControl(capture: ICaptureGrab) = 
        inherit BaseCameraConsumer(capture)
                               
        override x.ImageGrabbedHandler with get () : byte[] =
                                                                use mat = new Mat()
                                                                //capture.SetCaptureProperty(CapProp.FrameWidth, 300.0) |> ignore
                                                                //capture.SetCaptureProperty(CapProp.FrameHeight, 200.0) |> ignore
                                                                Console.WriteLine("DEBUG: Attempt to retrieve frame")
                                                                let ret = capture.Retrieve(mat)
                                                                if(not ret) then 
                                                                    Console.WriteLine("NOT RET. Ignoring")
                                                                    ignore()

                                                                use smoothedGrayFrame = new Mat()
                                                                use smallGrayFrame = new Mat()
                                                                use cannyFrame = new Mat()
                                                                use grayFrame = new Mat()
                                                                if(mat.NumberOfChannels > 1) then 
                                                                    Console.WriteLine("DEBUG: Inside mat.NumberOfChannels > 1")                                                                    
                                                                    CvInvoke.CvtColor(mat, grayFrame, ColorConversion.Bgr2Gray)
                                                                    let captured = mat.ToImage<Bgra, byte>()   
                                                                    Console.WriteLine("DEBUG: Captured output: {0}", Convert.ToBase64String(captured.ToJpegData(90)))
                                                                    captured.ToJpegData(90)
                                                                    //stream.Write(captured.Bytes, System.Convert.ToInt32(stream.Length), captured.Bytes.Length)
                                                                                                                                             
                                                                else
                                                                    Console.WriteLine("DEBUG: Inside ELSE mat.NumberOfChannels < 1")                  
                                                                    
                                                                    
                                                                    
                                                                    let captured = mat.ToImage<Gray, byte>()
                                                                    Console.WriteLine("DEBUG: Captured output: {0}", Convert.ToBase64String(captured.ToJpegData(90)))
                                                                    //mat.CopyTo(grayFrame)
                                                                    //Console.WriteLine("DEBUG: Captured output: {0}", Convert.ToBase64String(captured.ToJpegData(90)))
                                                                    captured.ToJpegData(90)
                                                                    //stream.Write(, System.Convert.ToInt32(stream.Length), captured.Bytes.Length)                                                                                                                                                                                                                                    
//                                                                    CvInvoke.PyrDown(grayFrame, smallGrayFrame)
//                                                                    CvInvoke.PyrUp(smallGrayFrame, smoothedGrayFrame)                                                                   
//                                                                    CvInvoke.Canny(smoothedGrayFrame, cannyFrame, 100, 60)


                                                                                                            
                
      
            
        