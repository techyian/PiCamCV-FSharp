﻿namespace PiCamCV.TankSetup
    
    open Emgu.CV
    open Emgu.CV.CvEnum
    open Emgu.CV.Structure
    open System.IO
    open PiCamCV    
    open PiCamCV.Capture.Interfaces
    
    [<AbstractClass>]  
    type BaseCameraConsumer(capture: ICaptureGrab) as this = 
                
        abstract member ImageGrabbedHandler : camColour : CameraColour * camHeight : double * camWidth : double -> byte[] with get

        interface ICameraConsumer with
            member val CameraCapture = capture with get, set            
            member x.ImageGrabbedHandler = this.ImageGrabbedHandler(CameraColour.Bgr, 300.0, 300.0)
                