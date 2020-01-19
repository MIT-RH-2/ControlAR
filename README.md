![ControlAR logo](https://challengepost-s3-challengepost.netdna-ssl.com/photos/production/software_photos/000/914/479/datas/gallery.jpg)
# ControlAR

## Inspiration
ControlAR is an augmented reality tool built for the Magic Leap that allows the user to turn everyday objects in to game controllers. By using computer vision and machine learning, our app can identify various objects you can find around the house and assigns a minigame to them.

## What it does
ControlAR is many diverse in its applications. We wanted to show off this variety so we created an assorted mixture of minigames.

You can fly a plane around by simply pointing a pen. You can drive a robot around the room with a banana.

## How we built it
This initial version of ControlAR is made with Unity, written in C#, and deployed to Magic Leap. The Unity client can also run on Android, allowing it to be used as an alternate controller. Camera frames (YUV data in byte buffers) are streamed via UDP to a custom Python server we wrote, which is intended to as a bridge between the Magic Leap 1 and the deep learning runtime. We were able to build and configure PoseCNN (an open-source pose estimation framework implemented from a research paper) from source and run basic images through it on an Nvidia GPU with CUDA support. We started work on converting YUV frame buffers to RGB data that is compatible with PoseCNN, but ran out of time for integration toward the end of the hackathon. We intend to finish the PoseCNN integration at some point in the near future.

## Challenges we ran into
- Linux OS drivers
- Raspberry Pi
- Deep learning framework - CUDA

## Accomplishments that we're proud of
- Streaming to/from a remote server for CUDA-based PoseCNN Magic Leap deployment and feature exploration Cohesive teamwork

## What we learned
- Networked streaming of camera data
- Convolutional neural network integration
- Have GPU environments set up ahead of a hackathon for deep learning, just in case

## What's next for ControlAR
As aforementioned, ControlAR can be used for a diverse array of applications. We plan to continue to optimize ControlAR, while adding more minigames.

## Credits
[Thomas Suarez](https://tomthecarrot.com)
[Jay Hesslink]()
[Weston Bell-Geddes](https://westonbdev.com/)
