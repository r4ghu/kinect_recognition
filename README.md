# kinect_recognition
This application helps you train any new objects for recognition and use Kinect v2 for the real time object recognition.

A part of this code is used from http://eyesbot.com/blog/?preload=motion_tracking.txt under Creative Commons License. 

Working:

Make sure that before starting the application Kinect v2 is connected to the USB 3.0 port and Kinect SDK is installed.

After training the required object you can go to the recognition tab.

Now you can see two streams: Color and Depth.

You can click anywhere on the depth stream and get the Coordinates of the depth pixel selected and also its rescpective depth from Camera. 

For making life easy and to avoid mistakes due to Noise in the depth stream, you can set the maximum depth that you want to measure in the textbox provided. All the pixels that are beyond maximum depth will look white.

Now you can setup the Kinect and check the depth from the required object and make changes to the environment accordingly. Once you are done with the setup and distance click "Save Feed" button. If the Kinect v2 recognizes object in the scene it will show the Status text and start saving the Color Stream to 'My Pictures' Folder. You can change the folder to your required Location in the SaveColor() function.

Note: Make sure that you click on Save Feed again to stop saving the stream.
