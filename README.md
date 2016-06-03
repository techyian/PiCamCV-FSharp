# PiCamCV-FSharp
A port of @neutmute 's PiCamCV project written in F#. It currently makes use of the following technologies:

1) Alchemy WebSocket Server - https://github.com/Olivine-Labs/Alchemy-Websockets
2) EmguCV - http://www.emgu.com/wiki/index.php/Main_Page
3) ASP.NET
4) F#

## Project motivation
I wanted to find an imaging library which would be compatible with my Raspberry Pi which would form part of a larger project which I will discuss later.
I came across PiCamCV which made use of the EmguCV library and stated it was compatible with the Raspberry Pi.

I also wanted to try to learn F#, so this is my first attempt, and I will look at improving the code as I become more familiar and comfortable with F#.

## Getting Started

After cloning this repository, open the solution in Visual Studio. Build. Set the TankServer project as your startup project and run. This will load the Alchemy WebSocket
server that's used.

To browse to the Web Client, open IIS and add a new application. Set the location of the application to the TankWeb project folder. You may need to grant the application pool
access to the folder or you may receive an unauthorised error.


### What's next?
As mentioned above, this port forms a part of a larger project which I'm working towards currently which is a mini robotics project using a Tamiya tank
chassis. I aim to use the Raspberry Pi to control the motion of the tank, and this project will provide imaging while the tank is in motion.

You can read more on the project here: http://www.techyian.co.uk/f-emgu-cv-and-asp-net-robotics-project-part-1/

