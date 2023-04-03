# Cats are Liquid API
Some small useful APIs for modding [Cats are Liquid - A Better Place](https://store.steampowered.com/app/1188080)

## Manual installation
1. Install any dependencies this mod has (listed above)
2. Install this mod the same way as described in BepInExPack manual installation guide

**This mod is using [popcron/gizmos](https://github.com/popcron/gizmos),
which is licensed under [MIT](https://github.com/popcron/gizmos/blob/master/LICENSE)**

## Changelog

#### 0.2.8
* fixed an error when a prophecy doesn't exist

#### 0.2.7
* added editor verification bypass
* added editor name length bypass

api:
* fixed ApplyAllPatches logging
* fixed customizable patch description being null throwing an exception
* fixed being able to set customizable patch section to null (if you have nullables on)

#### 0.2.6
* refactored debug mode
* added editor bypass
* added custom prophecies api

#### 0.2.5
* added customization profiles

#### 0.2.4
* **abp 1.2.5 support:** removed graphy toggle (graphy got remvoed in 1.2.5)
* removed accidentally left over debug code, oops...

#### 0.2.3
* added some meta stuff
* added some ui apis

#### 0.2.2
* initial Thunderstore release
* added popcron/gizmos
* added camera zooming
* added graphy toggle
* fixed some debug mode bugs
