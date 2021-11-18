# Cats are Liquid API
Some small useful APIs for modding [Cats are Liquid - A Better Place](https://store.steampowered.com/app/1188080)

## Installation
1. Install [BepInEx](https://docs.bepinex.dev/articles/user_guide/installation) (x64)
2. Install [Configuration Manager](https://github.com/BepInEx/BepInEx.ConfigurationManager/releases/latest)
   by drag-and-dropping the folder from the downloaded archive into the game's folder
3. Install [Bepinex.Monomod.HookGenPatcher](https://github.com/harbingerofme/Bepinex.Monomod.HookGenPatcher) as described in its README
4. Get the MonoMod.exe for Bepinex.Monomod.HookGenPatcher from [here](https://github.com/MonoMod/MonoMod/releases/latest) (download the asset ending with net452.zip)
5. ~~(optional but recommended) Install [LighterPatcher](https://github.com/harbingerofme/LighterPatcher) as described in its README~~ apparently, this doesn't work, so nvm
6. Install [Cats are Liquid API](https://github.com/cgytrus/CalApi/releases/latest) the same way as Configuration Manager
   **(all the other mods are installed the same way, unless stated otherwise)**
6. Install the mods you want

## Contributing
1. Clone the repository
2. Put the missing DLLs into CalApi/libs (for a more detailed explanation,
   follow the [Plugin development](https://bepinex.github.io/bepinex_docs/master/articles/dev_guide/plugin_tutorial/1_setup.html)
   guide on the BepInEx wiki starting from Gathering DLL dependencies)

**This repo is using [popcron/gizmos](https://github.com/popcron/gizmos),
which is licensed under [MIT](https://github.com/popcron/gizmos/blob/master/LICENSE)**
