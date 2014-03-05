ImageSeamCarving
================

A small windows form application for image seam carving.

The goal is to effectively shrink an image by removing pixels that are the least important.


This application is built for carving image seams through a process of:

1. calculating the derivative gradient of each pixel
2. dynamically combing the image to heat map the highest value paths
3. taking the lowest cost path accross the image and removing those pixels


code structure is as follows:

1. The main driver code lives in MainWindow.xaml.cs
2. classes live in the classes folder
3. interfaces live in the interfaces folder
4. things that could be "mocked" should be coded to interfaces


The project can be run by:

1. importing to visual studio
2. running the project (it lives in a WPF currently)
3. click "choose image" and select an image
4. click "gradient" to calculate the gradient of the image
5. click "heat map" to calculate the heat map
6. choose a number of pixels by which to shrink the image
7. and click "carve" to see the result


Thanks for viewing! If you want more info on seam carving, search for some youtube videos. Really cool stuff!
