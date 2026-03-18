# library-of-babel
A simple audiobook player for listening to offline .m4b files. Made in C# using Avalonia UI.

**NOTE: This was a one-off thing for personal use, I will not be taking feature requests and any possible future changes will be for my own personal gain.**

This project started out as a curiosity about what an .m4b file was. From there, I tried to build a parser but quickly realized that due to the file format being a version of MPEG-4, I was essentially rebuilding ffmpeg and gave up.

Instead I started looking into how viable it would be to make a simple local player app. Around a weeks worth of work later, I have my little app and am pretty happy with it. In today's day and age though it always feels like a waste if it's not shared so why not put it up as a repo? (This way I can also use it as a portfolio reference...)

---

### Setting it up
This baby is ffmpeg all the way down so it's a given that you need to have it installed.
Having ffmpeg installed and set up on your $PATH is needed for the scanning and image fetching to work.

Verify that this is the case by opening up the terminal/cmd and typing: ```ffmpeg``` or ```ffmpeg --version```

From here you should be able to open the project in an editor of your choice, I used VSCode while making it cause of Avalonias preview extension. Calling ```dotnet run``` or clicking the start button should hopefully, if you have your fingers crossed start a version of the app.

### Building it
Now this is hopefully just as easy as calling ```dotnet build``` but as I personally use Linux and have only verified that platform, it may be a little more of a headache for you. The built application should, if things go well, end up in the *bin/Release/net10.0/* directory within the projects files. In theory this is the entire process so I think you can handle it!

### Some quirks
This is my first application developed using Avalonia UI and it was a *bit* of a struggle... There are tons of not great logic in terms of events but I'm managing to run the app with a library of about 72 audiobooks, so that's my benchmark. I also won't deny that in my frustration I did ask the **G**rand **P**estilence of **T**ech so some stuff I can not guarantee to be entirely kosher since I don't understand Avalonia enough and after this project, I hope that I never do.

Long version short, it's a bit rough and you may have issues with images not being properly set after the image-sync. Normally, you can just reboot the app and it will redo the images it missed and properly update the covers.
Along with this, there are some funny business with getting the state of the book you left off at, I should probably just save the exact timestamps somewhere but it's only visual and doesn't affect playback. Pressing play will have it properly update its timestamps.
