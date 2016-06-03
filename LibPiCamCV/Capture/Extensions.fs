namespace PiCamCV.Capture

module CaptureConfigExtensions = 
    open System
    open Emgu.CV.CvEnum
    open PiCamCV.Capture.Interfaces

    type CaptureConfig with
        static member GetCaptureProperties(capture: ICaptureGrab) = 
            let settings = new CaptureConfig()
            settings.Res.Height <- Convert.ToInt32(capture.GetCaptureProperty(CapProp.FrameHeight))
            settings.Res.Width <- Convert.ToInt32(capture.GetCaptureProperty(CapProp.FrameWidth))
            settings.Framerate <- Convert.ToInt32(capture.GetCaptureProperty(CapProp.Fps))
            settings.Monochrome <- Convert.ToBoolean(capture.GetCaptureProperty(CapProp.Monochrome))
            settings

module UnitExtensions = 
    open System
    open Emgu.CV.CvEnum    
    open PiCamCV.Capture.Interfaces

     type Unit with
        static member SetCaptureProperties(capture: ICaptureGrab, properties: CaptureConfig) = 
            capture.SetCaptureProperty(CapProp.FrameHeight, (double)properties.Res.Height) |> ignore
            capture.SetCaptureProperty(CapProp.FrameWidth, (double)properties.Res.Width) |> ignore
            let monochrome : double = 
                match properties.Monochrome with
                | true -> 1.0
                | false -> 0.0
            capture.SetCaptureProperty(CapProp.Monochrome, monochrome) |> ignore         
            ()
        
