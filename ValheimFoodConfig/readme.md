First time development setup
pull project from github: https://github.com/geraldjglasgow/ValheimFoodConfig
open in intellij rider

download and install vortex
https://www.nexusmods.com/site/mods/1?tab=files&file_id=4576

allow vortex to manage valheim mods

download ILSpy: https://github.com/icsharpcode/ILSpy
https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-8.0.407-windows-x64-installer
This tool allows you to see the valheim source code. does this work with rider?
view valheim code in ILSpy

under assembilies in the project 
add under valheim/BepInEx/core
OHarmoney
BepinEx

add under valheim/valheim_Data/Managed
assembly_valheim.dll
UnityEngine
UnityEngine.CoreModule

to add assembilies that need to be publicized then
this is for making private methods public so we can transpile
This must be done for every valheim game update or bepinx updates
download this https://github.com/CabbageCrow/AssemblyPublicizer
https://github.com/BepInEx/BepInEx.AssemblyPublicizer
use bepinix. something wrong when using cabbagecrow publicizer for a couple assemblies


drag bepinex core libs into mod libs folder
also drag valiheim_Data libraries into mod library folder

add all libs to project

-- possibly get scriptengine for live debugging

TOOLS
-- ConfigurationManager, ScriptEngine, and UnityExplorer
are .dll files that go in Bepinx/plugins folder

unity explorer hit f7 in game to inspect unity objects

if using vortex and bepinx preloader is failing, then 
download bepinex without vortex and drag and drop doorstop_config, start_game, start_server, and winhttp into valheim base folder
