Hey friend, Thanks for bying my asset!


This asset contains the Advanced Video Player that allows you to play your local videos or videos from server, 
control it, use it in VR/AR/WebGL, fully customize it because the code is commented and it's not a problem to modify it.
My asset is using TextMeshPro but you can easily change it to an usual Text and use it as well.



How to use it:

In SampleVideoManager scene you will find a canvas with VideoManager object and Reinit button. VideoManager is the main object,
which contains AdvancedVideoManager script and other objects.
To make the asset work - you only need to launch the method LoadVideoClip() or LoadVideoFromUrl() for video clip or url
in a AdvancedVideoManager script.


Now, about main settings:

In an AdvancedVideoManager script you'll find the main settings of the video player,
they are only set up when you call LoadVideoClip() or LoadVideoFromUrl() method:

-isLoop - is video should be looping
-needToClickToShowInterface - is need to click on video to show an interface with the buttons
-neverHideInterfaceWhileHover - is never hide interface while mouse is hovering the video
-saveVolumeInPlayerPrefs - is need to save volume settings in PlayerPrefs
-savePlaybackSpeedInPlayerPrefs - is need to save playback speed settings in PlayerPrefs
-masterVolume - master volume (0% - 100%)
-volumeStepOffset - volume step by pressing Up/Down arrows (0% - 100%)
-timeStepOffset - time step by pressing Left/Right arrows or double pressing on video Left/Right sides (0 seconds - infinity seconds)
-allowedPlaybackSpeeds - allowed playback speed list


F.A.Q.

If you are facing with some troubles in webgl with playing video - make sure you follow these steps:
-Loading from url: Make sure your video is available and the target server provides you with CORS access.
	If it doesn't help - open the console (by pressing F12) and check the exact error
-Loading from local: Make sure you create the url by Path.Combine and the video locates in your streaming access


Contacts:
Email: korobko416@gmail.com
Telegram: @Alexandr_Korobko