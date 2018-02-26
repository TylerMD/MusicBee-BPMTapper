# MusicBee-BPMTapper
This plugin allows users to manually tap the BPM while listening to music and save it to the file's BPM tag.

![Alt Text](https://github.com/TylerMD/MusicBee-BPMTapper/blob/master/screenshots/Screenshot1.PNG)

![Alt Text](https://github.com/TylerMD/MusicBee-BPMTapper/blob/master/screenshots/Screenshot2.png)
Tap - Click in time with music
Save - Save value to file's BPM tag
Reset - Clear value and start over
2x - Multiply the value by 2
/2 - Divide value by half

The Taps field shows the Tap count. Usually I click 8.
The Diff field shows the change in average due to most recent click. The lower this value, the more accurate the tapping is.


## Installation
- To avoid minor bugs, update MusicBee to at least v3.1 (September 16, 2017)
- Download and extract the plug-in's [latest release](https://github.com/TylerMD/MusicBee-BPMTapper/releases).
- Open MusicBee.
- In the "Preferences -> Plug-ins", click "Add Plugin" button in top-right corner.
- Browse for downloaded '.dll' file and load it.
- In the "Preferences -> Hotkeys", Setup your preferred hotkeys for Tap/Save/Reset.
  - I recommend Ctrl+1/Ctrl+2/Ctrl+3
- Get to work!


## Development
Want to contribute? Great! Make a pull request and I'll review and merge.

## About Editing
When Building the project, Visual Studio may have to be Run As Administrator to insert the .dll file in to Program Files directory

## Todos
 - Add uninstaller
 - Remove the Titlebar to reduce footprint.
 - Fix Bug: On some PCs, Holding down Ctrl+1 keeps pressing Tap into the 1000s.
 
## License
MIT

