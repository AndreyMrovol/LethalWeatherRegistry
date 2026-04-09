namespace WeatherRegistry.Compatibility
{
  internal class BrutalCompanyCompat(string guid, string version = null) : MrovLib.Compatibility.CompatibilityBase(guid, version)
  {
    public void Init()
    {
      if (!this.IsModPresent)
      {
        return;
      }
    }

    public float GetScrapValueMultiplier()
    {
      if (!this.IsModPresent)
      {
        return 1f;
      }

      return BrutalCompanyMinus.Minus.Manager.scrapValueMultiplier;
    }

    public float GetScrapAmountMultiplier()
    {
      if (!this.IsModPresent)
      {
        return 1f;
      }

      return BrutalCompanyMinus.Minus.Manager.scrapAmountMultiplier;
    }
  }
}
