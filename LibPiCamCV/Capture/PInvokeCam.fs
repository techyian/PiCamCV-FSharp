namespace PiCamCV.Capture
                
module CvInvokeRaspiCamCV = 
    open System.Runtime.InteropServices
    open System
    open Common.Logging
    open Emgu.Util
    open Emgu.CV
    open Emgu.CV.CvEnum
    open PiCamCV.Capture.Interfaces


    let osCheck = 
        let p = Environment.OSVersion.Platform.ToString()
        match p with
        | "4" | "6" | "128" -> "unix"
        | _ -> "other"



 #if UNIX
    // Use this for Pi USB mode: [<Literal>]let CVLibrary =  "opencv_videoio"
    [<Literal>]
    let CVLibrary = "raspicamcv";
    [<Literal>]
    let EntryPointCapture = "raspiCamCvCreateCameraCapture"
    [<Literal>]
    let EntryPointCapture2 = "raspiCamCvCreateCameraCapture2"
    [<Literal>]
    let EntryPointQuery = "raspiCamCvQueryFrame"
    [<Literal>]
    let EntryPointRelease = "raspiCamCvReleaseCapture"
    [<Literal>]
    let EntryPointGetProperty = "raspiCamCvGetCaptureProperty"
    [<Literal>]
    let EntryPointSetProperty = "raspiCamCvSetCaptureProperty"    
#else
    // Use this for Pi USB mode: 
    [<Literal>]
    let CVLibrary =  "opencv_videoio"
    //[<Literal>]
    //let CVLibrary : string = "cvextern";
    [<Literal>]
    let EntryPointCapture = "cvCreateCameraCapture"
    [<Literal>]
    let EntryPointCapture2 = "NOT SUPPORTED1"
    [<Literal>]
    let EntryPointQuery = "cvQueryFrame"
    [<Literal>]
    let EntryPointRelease = "cvReleaseCapture"
    [<Literal>]
    let EntryPointGetProperty = "cvGetCaptureProperty"
    [<Literal>]
    let EntryPointSetProperty = "cvSetCaptureProperty"
#endif
    
    /// <summary>
    /// Allocates and initialized the CvCapture structure for reading a video stream from the camera. Currently two camera interfaces can be used on Windows: Video for Windows (VFW) and Matrox Imaging Library (MIL); and two on Linux: V4L and FireWire (IEEE1394). 
    /// </summary>
    /// <param name="index">Index of the camera to be used. If there is only one camera or it does not matter what camera to use -1 may be passed</param>
    /// <returns>Pointer to the capture structure</returns>
    [<DllImport(CVLibrary, EntryPoint=EntryPointCapture, CallingConvention = CallingConvention.Cdecl)>]
    extern IntPtr cvCreateCameraCapture(int index)

    [<DllImport(CVLibrary, EntryPoint=EntryPointCapture, CallingConvention = CallingConvention.Cdecl)>]
    extern IntPtr cvCreateCameraCapture2(int index, CaptureConfig config);
       
    /// <summary>
    /// Grabs a frame from camera or video file, decompresses and returns it. This function is just a combination of cvGrabFrame and cvRetrieveFrame in one call. 
    /// </summary>
    /// <param name="capture">Video capturing structure</param>
    /// <returns>Pointer to the queryed frame</returns>
    /// <remarks>The returned image should not be released or modified by user. </remarks>
    [<DllImport(CVLibrary, EntryPoint=EntryPointQuery, CallingConvention = CallingConvention.Cdecl)>]    
    extern IntPtr cvQueryFrame(IntPtr capture);

    /// <summary>
    /// The function cvReleaseCapture releases the CvCapture structure allocated by cvCreateFileCapture or cvCreateCameraCapture
    /// </summary>
    /// <param name="capture">pointer to video capturing structure.</param>
    [<DllImport(CVLibrary, EntryPoint=EntryPointRelease, CallingConvention = CallingConvention.Cdecl)>]        
    extern void cvReleaseCapture(IntPtr capture);

    [<DllImport(CVLibrary, EntryPoint=EntryPointGetProperty, CallingConvention = CallingConvention.Cdecl)>]            
    extern double cvGetCaptureProperty(IntPtr capture, int property);

    [<DllImport(CVLibrary, EntryPoint=EntryPointSetProperty, CallingConvention = CallingConvention.Cdecl)>]                
    extern int cvSetCaptureProperty(IntPtr capture, int property, double value);
       
        
