# WeatherRegistry

A Lethal Company mod for controlling game's weather system.

## Features

- A system for registering custom weathers and weather effects
- Weight-based weather selection system
- Level-based weather filtering system
- Scrap value/amount multipliers based on active weather
- Editor support for creating new weathers
- Weather effect overrides for customizing existing weather visuals

## Editor components

WeatherRegistry provides Unity Editor components to help mod developers create custom weather systems:

- `WeatherDefinition`: Create new weathers without writing code, offering the same capabilities as code-based weather creation
- `EffectOverride`: Design custom weather effect overrides directly in the Unity Editor
- Visual tools for configuring weather properties, effects, and transitions

## Command support

- `Forecast` command for viewing probabilites of weathers

## Weight-based weather selection system

Contrary to the vanilla algorithm, this mod uses a weight-based system for selecting weathers. You can set the weights based on 3 criteria:

1. Level weight: the weight of the weather based on specific level
2. Weather-to-weather weight: the weight of the weather based on the previous weather
3. Default weight: the base weight of the weather

During the weather selection process, the algorithm will try to use one of the weights in the order listed above.

## For developers

Install the mod from [Nuget](https://www.nuget.org/packages/mrov.WeatherRegistry):

```xml
<PackageReference Include="mrov.WeatherRegistry" Version="*-*" />
```

To install the mod in Unity Editor, add WeatherRegistry and [MrovLib](https://github.com/AndreyMrovol/LethalMrovLib/releases) dlls to your project. To create a new weather, create a new `WeatherDefinition` object.

## License

This project is licensed under [GNU Lesser General Public License v3.0](https://github.com/AndreyMrovol/LethalWeatherRegistry/blob/main/LICENSE.md).

## Credits

Thank you to everyone who contributed to this project, reported bugs and suggestions! Special thanks to:

- [Generic](https://thunderstore.io/c/lethal-company/p/Generic_GMD/) and [s1ckboy](https://thunderstore.io/c/lethal-company/p/s1ckboy/) for helping me test editor addons and providing feedback
- [XuXiaolan](https://thunderstore.io/c/lethal-company/p/XuXiaolan/) for helping me test the mod on various occasions
- [Beanie](https://thunderstore.io/c/lethal-company/p/Beaniebe/), [Monty](https://thunderstore.io/c/lethal-company/p/super_fucking_cool_and_badass_team/Biodiversity/), [Kenji](https://thunderstore.io/c/lethal-company/p/rectorado/), [Autumnis](https://thunderstore.io/c/lethal-company/p/Autumnis/) for helping me playtest the mod _a lot of times_
- [Zigzag](https://thunderstore.io/c/lethal-company/p/Zigzag/) for sending me multiple detailed bug reports

Code used in this project is based on the following works:

- [LethalCompanyTemplate](https://github.com/LethalCompany/LethalCompanyTemplate) (licensed under [MIT License](https://github.com/LethalCompany/LethalCompanyTemplate/blob/main/LICENSE))
- [LethalLib](https://thunderstore.io/c/lethal-company/p/Evaisa/LethalLib/) (licensed under [MIT License](https://github.com/EvaisaDev/LethalLib/blob/main/LICENSE))
- [LethalLevelLoader](https://thunderstore.io/c/lethal-company/p/IAmBatby/LethalLevelLoader/) (licensed under [MIT License](https://github.com/IAmBatby/LethalLevelLoader/blob/main/LICENSE))
- [LC-SimpleWeatherDisplay](https://thunderstore.io/c/lethal-company/p/SylviBlossom/SimpleWeatherDisplay/) (licensed under [MIT License](https://github.com/SylviBlossom/LC-SimpleWeatherDisplay/blob/main/LICENSE))
