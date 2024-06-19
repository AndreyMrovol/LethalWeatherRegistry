# 0.1.2

- fixed an error with WeatherRegistry crashing when there were more than 1 weather to register (thanks, xuxiaolan)

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

- added a check to SunAnimator to verify if custom weather has an animator bool defined (thanks, PureFPSZac)
- added a check to SunAnimator to honor the blacklist setting (although the result should be virtually the same)
- possibleWeathers table displayed during startup is now sorted correctly
- fixed the default filtering options not having a semicolon at the end
- added an option to blacklist vanilla weathers from happening on selected moons

# 0.0.5

- added an check to SunAnimator to check if there are any sun animator clips (thanks, finembelli)

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
