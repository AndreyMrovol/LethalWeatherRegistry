# 0.4.2

- (hopefully) fixed weather effects not enabling correctly on progressing weathers (thanks, `glacialstage`!)
- fixed weather effects re-enabling themselves in an infinite loop (thanks, `xuxiaolan`!)
- fixed config resolvers not ignoring duplicate entries (thanks, `llkiur`!)

# 0.4.1

- fixed an issue with negative index being called in an array (thanks, `iam_sympathy`!)

# 0.4.0

- added `WeatherMatcher` and `LevelMatcher` (not fully implemented yet!)
- changed `WeatherSync` to allow syncing multiple weather effects (thanks, `xuxiaolan`!)
- fixed an issue with WeatherRegistry disabling JWeatherOverrides (thanks, `nikkidarkmatter`!)
- fixed an issue with Orbits compatibility patch (thanks, `fiufki`!)

# 0.3.16

- added `WeatherResolvable` classes

# 0.3.15

- added a check to `LungProp` patch to check if scrap multipliers are active
- fixed an issue with `EntranceTeleportPatch` not resetting after lobby reload (thanks, `purpletheproto`!)

# 0.3.14

- redone apparatus patch
- fixed SunAnimator not resetting correctly between moons (thanks, `voxx`!)

# 0.3.13

- fixed a compatibility issue with `FacilityMeltdown` where the scrap multiplier would be applied twice
- added a `WeatherManager.WeatherDisplayOverride` method to override displayed weather condition

# 0.3.12

- bumped MrovLib to fix string resolving issues
- publicized config entries in `RegistryWeatherConfig`
- added an example class [`WeatherModExtendedConfig`](https://github.com/AndreyMrovol/LethalWeatherRegistry/blob/main/WeatherRegistry/Modules/WeatherModExtendedConfig.cs) to showcase extending `RegistryWeatherConfig` for mod developers

# 0.3.11

- made proper use of `Settings` class (fix proposal to [#13](https://github.com/AndreyMrovol/LethalWeatherRegistry/issues/13))
- bumped MrovLib

# 0.3.10

- readded "_small things that add content to the game_" since it was missing for a long time
- changed config description of `LevelFilters` to be more understandable (thanks, `autumnis`)
- added `FilteringOption` constructor to `BooleanConfigHandler`
- publicized `Weather.Config` (thanks, `voxx`!)

# 0.3.9

- fixed `WeatherEffectController` introduced in v0.3.8

# 0.3.8

- added `BeforeSetupStart` event
- `ConfigHandler` now uses `ConfigFile` field instead of Registry's `ConfigManager`
- added alternative `ConfigHandler` constructor
- publicized `ConfigHandlers` in WeatherRegistry

# 0.3.7

- fixed WeatherRegistry using saved weathers after getting fired (thanks, `lunxara`!)

# 0.3.6

- redone patch for LobbyControl's `lobby clear` command (fixes [#11](https://github.com/AndreyMrovol/LethalWeatherRegistry/issues/11))
- renamed `WeatherManager.currentWeathers` to `WeatherManager.CurrentWeathers` (again) in a backwards-compatible way
- added a check to `WeatherController.ChangeWeather` to prevent setting weather effects while in orbit
- fixed an issue with `SunAnimator` not changing animator clip for clear weather (thanks, `zigzagawaka`!)
- fixed an issue with setting new weather effects while inside a dungeon (thanks, `zigzagawaka`!)

# 0.3.5

- added compatibility for JLL's `WeatherOverride` (thanks, `jacobg5`!)
- WeatherRegistry will now disable weather effects during weather registration (thanks, `hasoeni`!)
- fixed Registry destroying WeatherTweaks special weathers during lobby reload
- changed some file names

# 0.3.4

- fixed my terrible mistake of enabling every weather object despite the currently set weather

# 0.3.3

- (hopefully) fixed all issues with `IndexOutOfRangeException` when accessing `TimeOfDay.effects` array [#11](https://github.com/AndreyMrovol/LethalWeatherRegistry/issues/11) (thanks: `pineappleguy03`, `explodingturtles456`, `CoolLKKPS` !)

# 0.3.2

- weather effects are now correctly synced between clients
- weather effects are now correctly re-enabled when leaving dungeon

# 0.3.1

- fixed weather effects not enabling properly ([#10](https://github.com/AndreyMrovol/LethalWeatherRegistry/issues/10)) (thanks: `zigzagawaka`, `kidnapperproot`!)

# 0.3.0

- Apparatice will now use WeatherRegistry's scrap value multiplier
- added _default weight_ and _level weight_ logs to startup
- added a check to loading weathers from save in case mod list was changed between lobby reloads (thanks, `lunxara`!)
- added `LevelWeatherTypes` property to `WeatherManager`
- added `GetCurrentLevelWeather` method to `WeatherManager`
- added null checks for `Weather.Effect`
- added an option to change which weather selection algorithm is used
  - used algorithm will be logged in the console
- added `BooleanConfigHandler` type and changed `WeatherConfig` to use it
- changed `screenLevelDescription` to use `.SetText()` (thanks, `iambatby`!)
- changed the transpilers of vanilla methods so only WeatherRegistry enables weather effects
- changed how disabled `ConfigHandler` behaves
  - before it would create a dummy config entry with a description that it's disabled
  - now it will not create the config entry at all
- fixed an error introduced in 0.2 with `Weather` properties not being used for default config values (sorry, `voxx`!)
- Weathers registered by WeatherTweaks will no longer be removed from the registered weathers list
- marked some properties as virtual

# 0.2.9

- added default weight and level weight logs to startup

# 0.2.8

- added logs for loading weathers from save

# 0.2.7

- fixed missing config defaults from previous version (thanks, `lunxara`!)

# 0.2.6

- added `CurrentEffectTypes` property to `WeatherManager` (thanks, `zaggy1024`!)

# 0.2.5

- changed how configs are handled internally - old properties are marked as obsolete to give you time to migrate
- fixed an issue with loading saved weathers after changing the moon list (thanks: `ericthetoon`, `darmuh`)
- added `EffectActive` property to `ImprovedWeatherEffect` (thanks, `zaggy1024`)

# 0.2.4

- fixed wrong name of the `TimeOfDay` logger
- added try-catch to `SunAnimator` to (hopefully) prevent soft-locking the game during landing (thanks: `call.me.doc`, `autumnis`)

# 0.2.3

- fixed an issue with Registry trying to set weather on a non-existing level (thanks: `glacialstage`, `darmuh`!)

# 0.2.2

- picked weathers are now saved to the savefile and will be restored on lobby reload

# 0.2.1

- fixed weather desyncs on first day (thanks: `lunxara`, `moroxide`!)
- (hopefully) fixed an issue with game reset soft-locking the game (thanks: `jennysbrood`!)

# 0.2.0

## Please re-generate your configs!

- merged [#7: refactor: modify many LogWarnings -> LogDebugs](https://github.com/AndreyMrovol/LethalWeatherRegistry/pull/7) (thanks, `mamallama`!)
- fixed `WeatherController` not syncing weather changes correctly (thanks, `giosuel`!)
- fixed an issue with [LobbyControl](https://thunderstore.io/c/lethal-company/p/mattymatty/LobbyControl/)'s `lobby clear` command spawning weather **`6`**
- changed how weather->weather weights are defined to be more intuitive:
  - previously setting `None@200` in Rainy meant that if None was the previous weather, Rainy is set to 200 weight
  - now setting `None@200` in Rainy means that if Rainy was the previous weather, None is set to 200 weight
  - updated the config entry description to reflect the change
- added a toggle for using Registry's scrap multipliers
- added a toggle between Registry's _weighted_ weather selection and a vanilla algorithm
- added `WeatherSelectionAlgorithm` class for allowing custom weather selection algorithms in the future
- added `CurrentWeathers` class for easier management of selected weathers **(this is a breaking change for `WeatherManager.CurrentWeathers`!)**
- added methods allowing to modify randomWeathers to `WeatherController`: `SetRandomWeathers`, `AddRandomWeather`, `RemoveRandomWeather`
- added `Enabled` property to `ConfigHandler`: weather developers can now opt-out of the Registry config entries from being used (thanks, `xuxiaolan`!)
- added `WeatherEffectOverride` class for defining weather effect overrides

# 0.1.25

- reverted 0.1.24 changes

# 0.1.24

- changed how scrap multipliers are applied: they are not replacing other mod values' anymore

# 0.1.23

- fixed an issue with **_`6`_** weather on Experimentation (thanks: `_v0xx_`, `giosuel`!)

# 0.1.22

- changed how override weathers are logged

# 0.1.21

- added level placeholders for config entries:
  - `All`: all levels
  - `Modded`/`Custom`: all modded levels
  - `Vanilla`: all vanilla levels
  - `Company`: the company level

# 0.1.20

- fixed an issue with `StartOfRound.OnDisable` not disabling WorldObjects (thanks, `_v0xx_`!)

# 0.1.19

- fixed null references when resolving SelectableLevels names

# 0.1.18

- added `ShipLanding` event

# 0.1.17

- updated MrovLib's ConfigHandler implementation (sorry for that!)
- changed the way ship's map screen displays planet info

# 0.1.16

- WeatherRegistry's scrap value/amount multipliers will be prioritized
- added `MapScreenUpdated` event

# 0.1.15

- fixed `StartOfRound.OnDisable` patch not firing correctly

# 0.1.14

- improved resolving planet and weather names that have spaces in them

# 0.1.13

- fixed `SetupFinished` event firing correctly

# 0.1.12

- fixed an issue with Nuget propagating wrong LC dependency version further (thanks, `giosuel`)

# 0.1.11

- MrovLib is now a required dependency
- `WeatherChanged` event is now properly invoked when new weathers are picked

# 0.1.10

- fixed an issue with randomWeathers having multiple entries of the same modded weather (thanks, `xuxiaolan`)

# 0.1.9

- added config options to disable logging startup logs & weather selection logs

# 0.1.8

- optimized the weight-picking algorithm (thanks, `whitespike`)
- changed some logs

# 0.1.7

- LevelWeights and WeatherWeights config options are now functional
- redid the ConfigHandler completely (it's cursed)
- added a set of defaults for vanilla weather>weather transitions (this requires testing and feedback!)
- changed some logs
- weights are now clamped between 0 and 10000
- added new options for weather makers: `DefaultLevelWeights` and `DefaultWeatherToWeatherWeights`

# 0.1.6

- fixed an error with config strings not being resolved correctly (thanks, `xuxiaolan`)
- changed some logs

# 0.1.5

- changed the scale of weather multipliers from (0,10000) to (0,100)

# 0.1.4

- added ScrapAmountMultiplier, ScrapValueMultiplier config options
- added LevelWeights and WeatherWeights config options (they don't do anything **yet**)
- fixed an error with WeatherRegistry not disabling registered weathers' effects when leaving lobby (thanks: `endoxicom`, `xuxiaolan`, `lunxara`)

# 0.1.3

- (hopefully) fixed an error with WeatherRegistry not disabling registered weathers' effects when leaving lobby (thanks, `endoxicom`)

# 0.1.2

- fixed an error with WeatherRegistry crashing when there were more than 1 weather to register (thanks, `xuxiaolan`)

# 0.1.1

- fixed an issue with LethalLib weathers not being registered correctly on modded moons
- fixed an issue with WeatherRegistry weathers being destroyed on joining lobbies
- (hopefully) fixed a Dictionary issue with modded weathers on clients
- renamed events: `setupFinished` -> `SetupFinished`, `weatherChanged` -> `WeatherChanged`
- added `DisableAllWeathers` event
- added `WeatherController` for changing weathers
- changed some logs
- changed possibleWeathers table to be sorted using enum values
- changed WeatherRegistry to forcefully remove all "None" weather entries from randomWeathers

# 0.1.0

### Please re-generate your configs!

- added a check to SunAnimator to verify if custom weather has an animator bool defined (thanks, `PureFPSZac`)
- added a check to SunAnimator to honor the blacklist setting (although the result should be virtually the same)
- possibleWeathers table displayed during startup is now sorted correctly
- fixed the default filtering options not having a semicolon at the end
- added an option to blacklist vanilla weathers from happening on selected moons

# 0.0.5

- added an check to SunAnimator to check if there are any sun animator clips (thanks, `finembelli`)

# 0.0.4

- fixed an error with WeatherRegistry keeping references to previous lobby weathers
- changed weather fields

# 0.0.3

- added config options for setting the default weights and level filtering
- fixed an issue with LethalLib hard dependency
- added an event system

# 0.0.2

- fixed an error preventing people from joining online lobbies

# 0.0.1

- hello! 👋
