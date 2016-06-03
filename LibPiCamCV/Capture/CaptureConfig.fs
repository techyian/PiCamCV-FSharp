namespace PiCamCV.Capture

    open System.Drawing
    open System
    open PiCamCV
    open Emgu.CV.CvEnum

    (* The Resolution class is, as the name suggests, allows us to manipulate the resolution of the image captured by Emgu CV. *)
    type Resolution(width: int, height: int) = 
                                
        member val Width = width with get, set
        member val Height  = height with get, set
        
        new() = Resolution(0, 0)

        override x.GetHashCode() : int = x.Width * 10000 + x.Height

        override x.Equals(yobj : obj) = 
            match yobj with
            | :? Resolution as y -> (x = y)
            | _ -> false
      
        override this.ToString() = String.Format("{0}x{1}", this.Width, this.Height)

        member this.GetCenter() = new Point(this.Width / 2, this.Height / 2)
    
    (* This is the main config class that we can manipulate the image captured by EmguCV. *)
    type CaptureConfig(res: Resolution, br: int, fr: int, monochr: bool) =

        member val Res = res with get, set
        member val Bitrate = br with get, set
        member val Framerate = fr with get, set
        member val Monochrome = monochr with get, set

        override this.ToString() = String.Format("res={0}, bitrate={1}, framerate={2}, monochrome={3}"
                                                                                        ,this.Res
                                                                                        ,this.Bitrate
                                                                                        ,this.Framerate
                                                                                        ,this.Monochrome)
                                        

        new() = new CaptureConfig(new Resolution(), 0, 0, false)

