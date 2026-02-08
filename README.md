# WeatherRegistry

<div style="display: flex; gap: 10px; flex-wrap: wrap; margin-bottom: 16px;">
  <img src="https://img.shields.io/codefactor/grade/github/andreymrovol/LethalWeatherRegistry?style=flat&logo=codefactor&logoColor=white&color=83E6FB&cacheSeconds=1200" alt="CodeFactor Grade">
  <img src="https://img.shields.io/thunderstore/dt/mrov/WeatherRegistry?style=flat&logo=thunderstore&logoColor=white&color=83E6FB&cacheSeconds=1200" alt="Thunderstore Downloads">
  <img src="https://img.shields.io/github/actions/workflow/status/AndreyMrovol/LethalWeatherRegistry/build.yml?branch=main&style=flat&logo=github&logoColor=white&color=83E6FB&cacheSeconds=1200" alt="GitHub Workflow Status">
  <img src="https://img.shields.io/github/v/release/AndreyMrovol/LethalWeatherRegistry?style=flat&logo=github&logoColor=white&color=83E6FB&cacheSeconds=1200" alt="GitHub Release Version">

  </div>
<br><br>

**A mod for Lethal Company allowing you to control the game's weather system.**

## Features

- A system for registering custom weathers and weather effects
- Weight-based weather selection system
- Weather-to-weather transition weights for creating weather progression patterns
- Level-based weather filtering system
- Scrap value/amount multipliers based on active weather
- Editor support for creating new weathers
- Multiple weather selection algorithms (Registry weighted, Vanilla, and Hybrid)
- Hot-reloadable configuration system that applies changes after completing the current day
- Terminal commands for forecasting and changing weather conditions
- and many more!

## Editor components

WeatherRegistry provides Unity Editor components to help mod developers create custom weather systems:

- [`WeatherDefinition`](https://lethal.mrov.dev/weatherregistry/references/weatherdefinition/): Create new weathers without writing code, offering the same capabilities as code-based weather creation
- [`ImprovedWeatherEffect`](https://lethal.mrov.dev/weatherregistry/references/improvedweathereffect/): Create custom weather effects with WeatherRegistry
- `EffectOverride`: Design custom weather effect overrides directly in the Unity Editor
- Visual tools for configuring weather properties, effects, and transitions

## Terminal Commands

WeatherRegistry supports terminal commands for managing and debugging weathers:

- `weather forecast <moon>` for viewing probabilites of weathers
- `weather change <weather>` for changing the current weather (host only)

## Weight-based weather selection system

WeatherRegistry uses a priority-based weight system instead of vanilla's hardcoded weather selection. Each weather can have multiple weight configurations, and the algorithm selects which one to use based on availability (checked in order):

1. **Level-specific weight**: Weight assigned to a specific moon (e.g., "Experimentation@200")
2. **Weather-to-weather weight**: Weight based on the previous day's weather (e.g., if yesterday was Foggy, today's Rainy weather uses its "after Foggy" weight)
3. **Default weight**: Fallback weight used when no specific conditions match

**Example:** If you want Rainy weather to be:

- Common on Experimentation (weight: 200)
- Very likely after Foggy weather (weight: 150)
- Rare otherwise (default weight: 25)

The algorithm automatically uses the most specific applicable weight during selection.

## For developers

Install the mod from [Nuget](https://www.nuget.org/packages/mrov.WeatherRegistry):

```xml
<PackageReference Include="mrov.WeatherRegistry" Version="*-*" />
```

To install the mod in Unity Editor, add WeatherRegistry and [MrovLib](https://github.com/AndreyMrovol/LethalMrovLib/releases) dlls to your project. To create a new weather, create a new `WeatherDefinition` object. For more information, check out the ["Creating and bundling weathers" guide (WIP)](https://lethal.mrov.dev/weatherregistry/guides/bundling/).

## License

This project is licensed under [GNU Lesser General Public License v3.0](https://github.com/AndreyMrovol/LethalWeatherRegistry/blob/main/LICENSE.md).

## Credits

Thank you to everyone who contributed to this project, reported bugs and suggestions! Special thanks to:

- [Generic](https://thunderstore.io/c/lethal-company/p/Generic_GMD/) and [s1ckboy](https://thunderstore.io/c/lethal-company/p/s1ckboy/) for helping me test editor addons and providing feedback
- [XuXiaolan](https://thunderstore.io/c/lethal-company/p/XuXiaolan/) for helping me test the mod on various occasions
- [Beanie](https://thunderstore.io/c/lethal-company/p/Beaniebe/), [Monty](https://thunderstore.io/c/lethal-company/p/super_fucking_cool_and_badass_team/Biodiversity/), [Kenji](https://thunderstore.io/c/lethal-company/p/rectorado/), [Autumnis](https://thunderstore.io/c/lethal-company/p/Autumnis/) for helping me playtest the mod _a lot of times_
- [Zigzag](https://thunderstore.io/c/lethal-company/p/Zigzag/) for sending me multiple detailed bug reports

and to **everyone** submitting their bug reports and testing the releases!

<br>

Code used in this project is based on the following works:

- [LethalCompanyTemplate](https://github.com/LethalCompany/LethalCompanyTemplate) (licensed under [MIT License](https://github.com/LethalCompany/LethalCompanyTemplate/blob/main/LICENSE))
- [LethalLib](https://thunderstore.io/c/lethal-company/p/Evaisa/LethalLib/) (licensed under [MIT License](https://github.com/EvaisaDev/LethalLib/blob/main/LICENSE))
- [LethalLevelLoader](https://thunderstore.io/c/lethal-company/p/IAmBatby/LethalLevelLoader/) (licensed under [MIT License](https://github.com/IAmBatby/LethalLevelLoader/blob/main/LICENSE))
- [LC-SimpleWeatherDisplay](https://thunderstore.io/c/lethal-company/p/SylviBlossom/SimpleWeatherDisplay/) (licensed under [MIT License](https://github.com/SylviBlossom/LC-SimpleWeatherDisplay/blob/main/LICENSE))
