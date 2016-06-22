module TankServer = 
    open System.Collections.Generic
    open PiCamCV.TankSetup
    open Alchemy.Classes
    open Newtonsoft.Json
    open Alchemy
    open System.Net
    open System
    open System.Threading
    open System.Threading.Tasks
    open PiCamCV.Capture
    open PiCamCV.Capture.Interfaces
    open System.Collections.Concurrent
    open System.Diagnostics

    type User(context: UserContext) = 
        member this.Context = context

    type UpdateValues = { Status: string; Width: int; Height: int; Colour: int  }
    type ReturnValues = { Type: string; Message: string}

    type WebSession() as this = 
           
        [<DefaultValue>]
        val mutable CamColour : CameraColour

        [<DefaultValue>]
        val mutable CamWidth : int

        [<DefaultValue>]
        val mutable CamHeight : int

        let compressedFrames = new ConcurrentQueue<string>()
               
        (* A list of our consumers - we have the ability to set up multiple consumers if we wish which we could use to manipulate the image
        frames differently from the same camera. *)
        let mutable consumers = new List<BaseCameraConsumer>()

        (* Object lock for use in BroadcastMessage so we don't have race conditions to the SendMessage method. Only one call can be done to SendMessage
        at any given time or we will get an exception. *)
        let objLock = new obj()

        //CancellationToken for our long running task 'StartCamera' and is used to signal cancelling of this task via its Cancel method.
        let cameraCancellationToken = new CancellationTokenSource()
      
        let onlineUsers = new List<User>();

        let broadcast(msgType: string, message: string) =
            for user in onlineUsers do
                let returnValue = { Type = msgType; Message = message}
                let stopwatch = new Stopwatch()
                stopwatch.Start()
                                
                user.Context.Send(JsonConvert.SerializeObject(returnValue), false, false)
        
                stopwatch.Stop()
                Console.WriteLine("PERF: Sending to client: {0}", stopwatch.Elapsed)

        let onReceiveDelegate : OnEventDelegate = new OnEventDelegate(fun (context: UserContext) -> 
                                                                        try
                                                                            let json = context.DataFrame.ToString();                                                                          
                                                                            let deObj = JsonConvert.DeserializeObject<UpdateValues>(json)                                                                            
                                                                            
                                                                            match deObj.Colour with
                                                                            | 0 -> this.CameraColour <- CameraColour.Gray
                                                                            | 1 -> this.CameraColour <- CameraColour.Bgr
                                                                            | 2 -> this.CameraColour <- CameraColour.Bgra
                                                                            | 3 -> this.CameraColour <- CameraColour.Hsv
                                                                            | 4 -> this.CameraColour <- CameraColour.Hls
                                                                            | 5 -> this.CameraColour <- CameraColour.Lab
                                                                            | 6 -> this.CameraColour <- CameraColour.Luv
                                                                            | 7 -> this.CameraColour <- CameraColour.Xyz
                                                                            | 8 -> this.CameraColour <- CameraColour.Ycc
                                                                            | _ -> this.CameraColour <- CameraColour.Bgr

                                                                            this.CameraHeight <- deObj.Height
                                                                            this.CameraWidth <- deObj.Width

                                                                        with
                                                                            | :? Exception as e -> 
                                                                                context.Send(JsonConvert.SerializeObject(e.Message))                                                                                
                                                                        )
        
        let onConnectDelegate : OnEventDelegate = new OnEventDelegate (fun (context: UserContext) -> 
                                                                        let user = new User(context)
                                                                        onlineUsers.Add(user)
                                                                        )            
        
        let onSendDelegate : OnEventDelegate = new OnEventDelegate (fun (context: UserContext) -> 
            Console.WriteLine(["Data Send To : "; context.ClientAddress.ToString()] |> String.concat "+" )
        )

        let onDisconnectDelegate : OnEventDelegate = new OnEventDelegate(fun (context: UserContext) -> 
            let user = query {
                    for usr in onlineUsers do
                    where (usr.Context.ClientAddress = context.ClientAddress)
                    select usr
                    exactlyOne
                    }                        
            match box user with 
            | null -> ()
            | _    -> 
                onlineUsers.Remove(user) |> ignore
                broadcast("info", "User disconnected")
        ) 
        
        (* Member functions to change the Colour/Height/Width of image on the fly. *)
        member this.CameraColour with 
                                        get () = this.CamColour and 
                                        set (value) = 
                                            Console.WriteLine("DEBUG: Updating camera colour")
                                            this.CamColour <- value
        member this.CameraHeight with 
                                        get () = this.CamHeight and 
                                        set (value) = 
                                            Console.WriteLine("DEBUG: Updating camera height")
                                            this.CamHeight <- value
        member this.CameraWidth with 
                                        get () = this.CamWidth and 
                                        set (value) = 
                                            Console.WriteLine("DEBUG: Updating camera width")
                                            this.CamWidth <- value

        member this.CompressedFrames = compressedFrames
            
        (* The CaptureConfig class is used to hold individual properties which can be used to manipulate the image returned via EmguCV. The properties
        in this class include setting the width/height of the image and bitrate/framerate if we so wish. *)
        member this.GenerateCaptureConfig() : CaptureConfig = 
            let config = new CaptureConfig(new Resolution(100, 100), 120, 24, true)          
            config

        (* The BroascastImage method is used to continuously send image frames from the server to the client when a 
        websocket handshake is gained. The method features object locking so we don't have race conditions to the SendMessage 
        method of our tank instance. *)
        member this.BroadcastImage() =             
            let del = Action<CancellationToken>(fun (token: CancellationToken) -> 
                let rec loop() =
                    if(not cameraCancellationToken.IsCancellationRequested) then                                                   
                        if(this.CompressedFrames.Count > 0) then
                            
                            let stopwatch = new Stopwatch()
                            stopwatch.Start()

                            let success, firstItem = this.CompressedFrames.TryDequeue()
                            
                            stopwatch.Stop()
                            Console.WriteLine("PERF: Dequeue: {0}", stopwatch.Elapsed)
                            
                            if(success) then                                
                                broadcast("image", firstItem)
                                                                                     
                        Thread.Sleep(200)
                        loop()
                loop()
                )
            Task.Factory.StartNew(fun () -> del.Invoke(cameraCancellationToken.Token), 
                                            cameraCancellationToken.Token, TaskCreationOptions.LongRunning, 
                                            TaskScheduler.Default);
          
        (* By calling upon our StartCamera method, we begin a task which will start sending image frames to the client. 
        This has the ability to send image frames to all tank instances i.e all clients. *)
        member this.StartCamera() = 
            let del = Action<CancellationToken>(fun (token: CancellationToken) -> 
                (*We're going to use this recursive function to only run while the WebSocket session is running. When we send a cancel request to the 
                cancellation token, we break out. *)
                let rec loop() =
                    if (not cameraCancellationToken.IsCancellationRequested) then
                        let cons = consumers.Item 0
                        match box cons with
                        | null ->  broadcast("info", "null")
                        | _    ->                                  
                                   let imageByteArray = cons.ImageGrabbedHandler(this.CameraColour, new Resolution(this.CameraHeight, this.CameraWidth))
                                                                      
                                   let base64Str = Convert.ToBase64String(imageByteArray)  
                                                                                               
                                   //Don't let the list get too big. 
                                   if(this.CompressedFrames.Count < 30) then                                                                                                        
                                       let stopwatch = new Stopwatch()
                                       stopwatch.Start()

                                       this.CompressedFrames.Enqueue(base64Str)
                                       stopwatch.Stop()
                                       Console.WriteLine("PERF: Enqueue: {0}", stopwatch.Elapsed)
                                             
                                                
                        (*Send the thread to sleep for a brief moment so BroadcastImage doesn't get overwhelmed. Without it, you'll find occasions where
                        the call to SendMessage results in an exception due to a request already being processed. *)                        
                        loop()
                loop()
                )
            Task.Factory.StartNew(fun () -> del.Invoke(cameraCancellationToken.Token), 
                                            cameraCancellationToken.Token, TaskCreationOptions.LongRunning, 
                                            TaskScheduler.Default);

        (* By passing in an instance of the ICaptureGrab object, we can create instances of our consumers within here. The consumers are then
        stored within our list of consumers. *)
        member this.SetupCameraConsumers(capture: ICaptureGrab) : unit =
            let basicCapture = new BasicCaptureControl(capture, this.CameraColour)
            consumers.Add(basicCapture)
        
        (* This is the method which is used to get an instance of the capture which acts as a handle on the camera. Once we have the handle, we can
        pass it into our SetupCameraConsumers method to setup our consumers. Once we have the handle, we can start capturing by calling the capture.start method. *)
        member this.SetupCapture (index, device: CaptureDevice) = 
            let request = new CaptureRequest()
            request.Config <- this.GenerateCaptureConfig()
            request.CameraIndex <- index
            request.Device <- device
            let capture = CaptureFactory.GetCapture request
            Console.WriteLine("DEBUG: Attempting to start camera")
            capture.Start()
                                  
            this.StartCamera() |> ignore
            this.BroadcastImage() |> ignore
            this.SetupCameraConsumers capture
            ignore

        member this.CreateServerInstance() =         
            let webSocketServer = new WebSocketServer(81, IPAddress.Any)
            webSocketServer.OnReceive <- onReceiveDelegate
            webSocketServer.OnSend <- onSendDelegate
            webSocketServer.OnConnected <- onConnectDelegate
            webSocketServer.OnDisconnect <- onDisconnectDelegate
            webSocketServer.TimeOut <- new TimeSpan(1, 0, 0)
            webSocketServer
        
    
    [<EntryPoint>]
    let main argv = 
                
        let webSession = new WebSession()
        let serverInstance = webSession.CreateServerInstance()        
        
        webSession.SetupCapture(0, CaptureDevice.Usb) |> ignore
        
        serverInstance.Start()
        
        
        let rec exitCommand() = 
            let line = Console.ReadLine()
            if(line <> "exit") then
                exitCommand()

        exitCommand()        

        serverInstance.Stop() 
        0 // return an integer exit code
