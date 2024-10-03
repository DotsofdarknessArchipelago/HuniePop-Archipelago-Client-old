# HunniePopArchipelago

Built using template from https://github.com/alwaysintreble/ArchipelagoBepInExPluginTemplate

Requires Hunie Pop APWorld ([link](https://github.com/DotsofdarknessArchipelago/HuniePop-APWorld)) to generate a world

A BepInEx plugin for Hunie Pop to connect and talk to a Archipelago server for multiworld randomization games

BACKUP YOUR SAVE FILE BEFORE USING THIS AS I CANT GUARANTEE THAT IT WILL NOT CORRUPT IT
(also is a good idea to back up your saves when modding any game)
- Windows save location: "C:/Users/{YOUR USERNAME}/AppData/LocalLow/HuniePot/HuniePop/"
- Mac save location: "/Users/{YOUR USERNAME}/Library/Application Support/com.HuniePot.HuniePop/"

INSTALLATION INSTRUCTIONS:

- Have Hunie Pop Installed
- Download BepInEx ([link](https://github.com/BepInEx/BepInEx/releases))(<b><ins>x86 version</ins>(32bit) x64 will not work</b>)(V5.4.23.2 recommend)(<b>THIS WILL NOT WORK ON BepInEX V6</b>)
- Download Hunie Pop Archipelago plugin (See Releases for latest version)
- Extract and copy the contents of BepInEx to the directory where "HuniePop.exe" is
- Extract and copy the contents of "Hunie Pop Archipelago plugin.zip" to the directory where "HuniePop.exe" is if it asks you to overwrite files click yes

NOTE if you get a game crash when starting the game make sure that in "{huniepop game directory}/bepinex/config/bepinex.cfg" the 2nd last option is "type = MonoBehaviour" <b><ins>not</ins></b> "type = Application"
