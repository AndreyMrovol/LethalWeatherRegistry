namespace WeatherRegistry.Definitions
{
  public class ImprovedRandomWeatherWithVariables : RandomWeatherWithVariables
  {
    public override string ToString()
    {
      return $"{this.weatherType}\t({this.weatherVariable}, {this.weatherVariable2})";
    }
  }
}
