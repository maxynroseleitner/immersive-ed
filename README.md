# Prismo - An affective computing platform
## Project Description

Emotion is a key part of what makes us all human and helps us relate with the world around us. Miscommunication can hinder our ability to express our feelings and enjoy the company of others. With Prismo, you can augment the way you interact with others in order to better keep track of and understand the emotional states of others. 

Our software is able to detect and analyze multiple modes of affective data such as facial expressions, word sentiment, and vocal tone in real time. This information is then synthesized and displayed using weather phenomenon that the user can more easily interpret.
## Building and deploying the Project on Hololens
Refer to the link below for building and deploying project in Unity and Hololens:

https://docs.microsoft.com/en-us/windows/mixed-reality/holograms-100

## For Setting up Networking on Hololens
### Receiver Settings
In order to enable facial analysis, start the AffectivaServer application and 
make the following changes to the Input Device Camera's Receiver 2 script:\
Set Refresh Time to 5\
Set Receiver Port to 8010 or a port of your choice. Make a note of this port number, as you will need it in the HoloLensApplication as well.

### Sender Settings
Main Camera -> Video Panel
Settings on Video Panel:
1.	Enable Log Data - Check this if you want to see following:\
	App’s Frame rate, Camera capture Rate, Number of Frames sent to the receiver, Size of the images
2.	View feed on Device:\
    a. 	This option will come handy if you want to check if the camera is working or not.\
    b.	Enabling this will display the camera feed right in front of you on a 2D texture.\
    c.	Flip Horizontal/Vertical : To adjust the camera feed. 
3.	Sending Data:\
	a.	Start Sending: Check it to start sending data to the receiver.\
	b.	Use Quality: Each frame is converted to jpeg before sending. If this option is unchecked then 			the default quality is 75. Check this to specify the quality.\
    c.	Buffer size: It is the size of the cache. Suppose if the buffer size 5, then 5 frames are cached
    	before sending. Minimum Buffer size is 1.\
    d.	Resize to Width: Since images taken from the camera can be very large, while converting it to jpeg, 		one can specify a custom width. The height will be adjusted automatically with respect to the 			aspect ratio.\
    e.	Requested Frame Rate: It is the rate at which the camera captures the video. For hololens, The 			available frame rates are 5, 15, 30.\
    f.	Reduce Frame Rate to: To reduce the frame rate further, this option can be used.
   
  Note: Make sure your receiver and sender are on the same network and ports are same on the applications

### TCP Networking:
Obtain the IP address of the computer running the AffectivaServer application. Add this IP address into Receiver IP address field of the TCPNetworking object. Also ensure that the Receiver Port Number is the same as the port number used for the AffectivaServer application.








