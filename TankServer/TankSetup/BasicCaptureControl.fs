namespace PiCamCV.TankSetup          
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
                                                                let ret = capture.Retrieve(mat)
                                                                if(not ret) then ignore()

                                                                use smoothedGrayFrame = new Mat()
                                                                use smallGrayFrame = new Mat()
                                                                use cannyFrame = new Mat()
                                                                use grayFrame = new Mat()
                                                                if(mat.NumberOfChannels > 1) then 
                                                                    CvInvoke.CvtColor(mat, grayFrame, ColorConversion.Bgr2Gray)
                                                                    let captured = mat.ToImage<Bgra, byte>()   
                                                                    captured.ToJpegData(90)
                                                                    //stream.Write(captured.Bytes, System.Convert.ToInt32(stream.Length), captured.Bytes.Length)
                                                                                                                                             
                                                                else                  
                                                                    let captured = mat.ToImage<Gray, byte>()
                                                                    mat.CopyTo(grayFrame)
                                                                    captured.ToJpegData(90)
                                                                    //stream.Write(, System.Convert.ToInt32(stream.Length), captured.Bytes.Length)                                                                                                                                                                                                                                    
//                                                                    CvInvoke.PyrDown(grayFrame, smallGrayFrame)
//                                                                    CvInvoke.PyrUp(smallGrayFrame, smoothedGrayFrame)                                                                   
//                                                                    CvInvoke.Canny(smoothedGrayFrame, cannyFrame, 100, 60)


                                                                                                            
                
      
            
        