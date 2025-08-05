namespace WeatherRegistry.Compatibility
{
  internal class MalfunctionsCompat(string guid, string version = null) : MrovLib.Compatibility.CompatibilityBase(guid, version)
  {
    private Logger Logger { get; } = new("Malfunctions", LoggingType.Debug);

    public bool IsNavigationalMalfunctionActive()
    {
      if (!this.IsModPresent)
      {
        return false;
      }

      Logger.LogDebug("Checking if navigational malfunction is active...");

      return Malfunctions.State.MalfunctionNavigation.Active;
    }

    public void SetNotifiedToFalse()
    {
      if (!this.IsModPresent)
      {
        return;
      }

      Logger.LogDebug("Setting navigational malfunction's NOTIFIED to false...");

      Malfunctions.State.MalfunctionNavigation.Notified = false;
    }
  }
}
