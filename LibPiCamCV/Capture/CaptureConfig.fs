namespace PiCamCV.Capture

    open System.Drawing
    open System
    open PiCamCV
    open Emgu.CV.CvEnum
    open System.Runtime.InteropServices

    (* The Resolution class is, as the name suggests, allows us to manipulate the resolution of the image captured by Emgu CV. *)
    [<type:StructLayout(LayoutKind.Sequential, Pack=1, CharSet=CharSet.Ansi)>]
    type Resolution = 
                
        val mutable Width : int
        val mutable Height : int

        new(width, height) = { Width = width; Height = height}

        override x.GetHashCode() : int = x.Width * 10000 + x.Height

        override x.Equals(yobj : obj) = 
            match yobj with
            | :? Resolution as y -> (x = y)
            | _ -> false
      
        override this.ToString() = String.Format("{0}x{1}", this.Width, this.Height)

        member this.GetCenter() = new Point(this.Width / 2, this.Height / 2)
    
    (* This is the main config class that we can manipulate the image captured by EmguCV. *)
    [<type:StructLayout(LayoutKind.Sequential, Pack=1, CharSet=CharSet.Ansi)>]
    type CaptureConfig =

        val mutable Res : Resolution
        val mutable Bitrate : int
        val mutable Framerate : int
        val mutable Monochrome : bool
                
        new(res: Resolution, br: int, fr: int, monochr: bool) = 
            { Res = res; Bitrate = br; Framerate = fr; Monochrome = monochr }
     
        override this.ToString() = String.Format("res={0}, bitrate={1}, framerate={2}, monochrome={3}"
                                                                                        ,this.Res
                                                                                        ,this.Bitrate
                                                                                        ,this.Framerate
                                                                                        ,this.Monochrome)