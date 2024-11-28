using Unity.Collections;
using Unity.Netcode;

namespace WeatherRegistry.Definitions
{
  public struct WeatherSyncData : INetworkSerializable
  {
    public LevelWeatherType Weather;
    public FixedString64Bytes LevelName;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer)
      where T : IReaderWriter
    {
      serializer.SerializeValue(ref Weather);
      serializer.SerializeValue(ref LevelName);
    }

    public override readonly string ToString()
    {
      return $"{this.LevelName.Value}:{this.Weather}";
    }
  }

  public struct WeatherSyncDataWrapper : INetworkSerializable
  {
    public WeatherSyncData[] Weathers;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer)
      where T : IReaderWriter
    {
      int length = 0;
      if (!serializer.IsReader)
      {
        length = Weathers?.Length ?? 0;
      }

      serializer.SerializeValue(ref length);

      if (serializer.IsReader)
      {
        Weathers = new WeatherSyncData[length];
      }

      for (int i = 0; i < length; i++)
      {
        serializer.SerializeValue(ref Weathers[i]);
      }
    }
  }

  public struct WeatherEffectSyncData : INetworkSerializable
  {
    public LevelWeatherType WeatherType;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer)
      where T : IReaderWriter
    {
      serializer.SerializeValue(ref WeatherType);
    }
  }
}
