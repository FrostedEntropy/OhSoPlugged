# OhSoPlugged - Plugin For 'Oh So Hero!'

This plugin written in C# will allow your adult toys connected through [Intiface Central (GUI)](https://intiface.com/central) to interact with the game [Oh So Hero!](https://store.steampowered.com/app/2086050/Oh_So_Hero/). 

## Disclaimer

This is my first ever game mod/plugin, and in C# which I'm not super familiar with, so don't expect senior programmer-level coding here. The code works™, but I'm certain there are better ways to handle various tasks. I just wanted to make this plugin since I thought it'd be a fun project. 

## What this plugin does and supports:

This plugin will allow your vibrators to well...vibrate. If your vibrator is supported by [Intiface Central (GUI)](https://intiface.com/central), then it should work. 
> Please note that for multifunction toys that support both vibration and other functionalities, only vibration will work with this plugin. 

In-game, the toy will vibrate when the following occurs: 
- When the player is attacked. 
- When the player receives a special charm attack. 
- When the player triggers an animation, either alone or with other NPCs.
- When an animation is played from the gallery (but only once). 

In order to avoid penalizing the player for letting the full animations play out, some HP is recovered to make up for the extra HP loss. 

## What this plugin doesn't do or doesn't support:

This plugin does not support toys with functionalities other than vibration. This means that toys like strokers will not work, but if the toy also has vibration, then the vibration should work. This may change in the future, as I'd like to be able to add support for toys like the Lovense Solace Pro, but I haven't been able to get it to work yet. 

There are some other limitations in-game:
- While scenes can be "cancelled" by the player, it's just cycling through the animations faster, so the toys will continue to vibrate until the final animation is stopped.  
- Animated loading screens and cutscenes are not currently supported. Will see if I can add this later, but support for cutscenes, like the final boss cutscenes, is looking tricky. 
- In the gallery, the toy will only vibrate for the full scene once. It will not loop or start over unless you move to a different animation and then back.  

## Installation / Requirements 

To get everything setup so that your toys can communicate with the game, please follow these instructions. 
1. Download and install [Intiface Central (GUI)](https://intiface.com/central) if you don't have it already. 
2. Download the appropriate version of [BepInEx](https://github.com/BepInEx/BepInEx/releases) for your operating system. 
    - BepInEx is what allows us to inject our plugin into Oh So Hero!
    - Documentation for BepInEx can be found [here](https://docs.bepinex.dev/articles/user_guide/installation/index.html). 
3. Place the content of the BepInEx zip file you downloaded directly inside your \Oh So Hero!\ folder. 
4. Launch Oh So Hero!, then close the game. This will cause BepInEx to generate the files that it needs to interact with the game. 
5. Download the [latest release](https://github.com/FrostedEntropy/OhSoPlugged/releases) for this plugin. 
6. Place all the files in the \Oh So Hero!\BepInEx\plugins\ folder. 
7. Launch Intiface Central and Start the server.
8. Make sure that your toys are connected.
    - Under the Devices tab, click Start Scanning to scan for your toys. 
9. Launch Oh So Hero!
    - You might see the game launch, close then relaunch again. It should be fine afterwards. I haven't been able to identify the cause, but will try to fix this in a future release. 

## Troubleshooting

I only have Lovense toys available, so my ability to assist with other toys is limited. With that said, if your toy disconnects frequently 
and it is not a Lovense toy, then consider modifying the RefreshRate in SexConfig.json to 1 instead of 0.5. This *might* help by reducing how
frequently we are making calls to Intiface/Toy, which could prevent some errors. 

For Lovense toys, I found that using the Lovense Connect app on my phone and connecting that to Intiface Central provided a much more stable
connection even with a refresh rate of 0.5. Consider using that app if you experience frequent disconnects when your toy is connected directly to Intiface Central. 
> Note: This may be because I'm using the default bluetooth driver for Windows 10, I've heard that it's not as reliable as a dedicated bluetooth USB adapter, but
I have not confirmed this myself. 

## Customizing your experience 

Amongst the files included in this plugin, you'll find a file called "SexConfig.json". This file contains:
- Some basic configuration:
    - ButtplugEnabled: 0 (disabled) or 1 (enabled). If disabled, toy integration will be disabled.
    - VibrationAlwaysOn: 0 (disabled) or 1 (enabled). If this setting is enabled, the toy will always vibrate even if you aren't in a scene or in combat. You do need to trigger combat or a scene once for this setting to activate. Useful feature if you tend to go soft outside of scenes or combat (like reading text, etc.)
    - VibrationAlwaysOnStrength: 0.0 (disabled) to 1.0. The strength of the vibration if VibrationAlwaysOn is enabled. I recommend keeping it low, like 0.1, as most scenes start at 0.2.  
    - MaxButtplugStrength: 0.0 to 1.0. If you don't like the toy being at max strength for some scenes, you can lower this value a little to lower the global vibration strength. 
    - RefreshRate: Default is 0.5 seconds. Affects how often the toy refreshes its instructions. I'd advise leaving it as is, but if you customize the scenes below, then you may need to adjust this value. 
    - PostSexHealthRegen: 0.0 (disabled) or greater (enabled). Default value is 1 and it acts as a multipler, so setting it to 2 will double the HP recovered, etc. If disabled, no health will be restored after a scene, which is the default game behaviour. Since scenes with enemies cause HP loss every second and the plugin runs the full length of a scene, this will prevent the player from being penalized too much for letting the scenes play out. 
    - LogAnimationNames: 0 (disabled) or 1 (enabled). Assuming this plugin still works after future game updates, instead of rebuilding the whole program, you can instead enable (1) this option to make the scene name visible in the BepInEx log and use the scene name to create a new SexScenesConfig for that animation below in the file. 
        -  You can enable the BepInEx console log by going to \Oh So Hero!\BepInEx\config\ and modifying BepInEx.cfg to set [Logging.Console] to true.  
- Animations Configuration: 
    - Animation Names: See LogAnimationNames above for more information. Animation names are entered in this order: NPC, Monster, Monster-Multi. 
    - Scene#: Where # is a number that starts at 1. There is no upper limit at the moment, but there's a chance things could break if you go a little crazy here. Each scenes corresponds to an animation loop that the characters will engage in for a certain duration before moving to the next scene. Each scenes has the following parameters: 
        - Strength: 0.0 to 1.0. Affects the vibration strength.
        - Duration: 0.5 or greater. Affects the duration of the vibration. 

> Before making any changes to this file, it is recommended that you make a copy of it. If you make a mistake, like missing a comma or something, then the game may run into some errors. Having a backup will allow you to restore everything to the default parameters.  

You can modify an existing animation by adding or removing scenes, and changing the strength or duration values. If you need to modify the duration values however, then you may need to consider modifying the RefreshRate if your duration has decimals that don't end with .0 or .5. 

If new characters are added in future updates, you can use one of the existing templates to more easily update the file. Simply CTRL+F for "TEMPLATE" and look for the appropriate version (NPC, MONSTER_TEMPLATE_Joe_CSP, etc.). Make a copy of this template, and modify the necessary fields. Don't forget the commas. Seriously. 
> You might feel the need to create many duplicates of the TEMPLATEs, but this will cause error if you run the code as it doesn't like duplicated dictionary entries. Consider giving them unique placeholder names if needed. 

Useful tips:
- Use a text editor like Notepad++ or a code editor like VS Code that displays row numbers to modify the SexConfig.json file. If there are errors in your file, like missing commas, the BepInEx Console log, if enabled, should tell you the row number, so this will help you a lot to fix any errors. 
- For calculating the Duration more accurately, I used a Stopwatch app on my phone. Each new scene can be counted as a Lap, so you can measure the duration of the whole animation more easily this way, and then convert each lap into a scene#. It's usually better to round down the time to .0 or .5 rather than up, and then you can make the necessary adjustments after testing in-game. There's an extra 1.5 seconds ramp down of the toy after a scene ends, so that should give you a bit more leeway. 

## Files included in this release
- Plugin files:
    - OhSoPlugged.dll
    - SexConfig.json
- Required libraries: 
    - Buttplug.dll
    - Newtonsoft.Json.dll
    - System.Data.dll
    - System.Runtime.Serialization.dll


## Licenses
[Buttplug C#](https://github.com/buttplugio/buttplug-csharp) is covered under the following BSD 3-Clause License:
```text

Copyright (c) 2016-2024, Nonpolynomial, LLC
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this
  list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice,
  this list of conditions and the following disclaimer in the documentation
  and/or other materials provided with the distribution.

* Neither the name of buttplug nor the names of its
  contributors may be used to endorse or promote products derived from
  this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
```

[Newtonsoft.JSON](http://newtonsoft.com/json) is covered under the
following MIT License:
```text
The MIT License (MIT)

Copyright (c) 2007 James Newton-King

Permission is hereby granted, free of charge, to any person obtaining
a copy of this software and associated documentation files (the
"Software"), to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to
permit persons to whom the Software is furnished to do so, subject to
the following conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
```