using System;
using GameNetcodeStuff;
using UnityEngine;

namespace WeatherRegistry.Editor
{
  [RequireComponent(typeof(AudioSource))]
  public class ImprovedOccludeAudio : MonoBehaviour
  {
    #region Internal Components
    private AudioSource thisAudio;
    private AudioLowPassFilter lowPassFilter;
    private AudioReverbFilter reverbFilter;
    private bool occluded;
    private float checkInterval;
    #endregion

    #region Low Pass Filter Settings
    [Header("Low Pass Filter")]
    [Tooltip("Override the automatic low pass filter cutoff frequency")]
    public bool overridingLowPass = false;

    [Tooltip("The cutoff frequency to use when override is enabled (higher values = less filtering)")]
    [Range(500f, 20000f)]
    public float lowPassOverride = 20000f;
    #endregion

    #region Reverb Settings
    [Header("Reverb Effects")]
    [Tooltip("Enable reverb effects when audio is played inside structures")]
    public bool useReverb = false;
    #endregion

    private Vector3 GetEffectivePosition()
    {
      return transform.position;
    }

    private void Start()
    {
      // Set up low pass filter
      lowPassFilter = GetComponent<AudioLowPassFilter>();
      if (lowPassFilter == null)
      {
        lowPassFilter = gameObject.AddComponent<AudioLowPassFilter>();
        lowPassFilter.cutoffFrequency = 20000f;
      }

      // Set up reverb if needed
      if (useReverb)
      {
        reverbFilter = GetComponent<AudioReverbFilter>();
        if (reverbFilter == null)
        {
          reverbFilter = gameObject.AddComponent<AudioReverbFilter>();
        }
        reverbFilter.reverbPreset = AudioReverbPreset.User;
        reverbFilter.dryLevel = -1f;
        reverbFilter.decayTime = 0.8f;
        reverbFilter.room = -2300f;
      }

      thisAudio = GetComponent<AudioSource>();

      // Initial occlusion check
      occluded = CheckOcclusion();

      checkInterval = UnityEngine.Random.Range(0f, 0.4f);
    }

    private void Update()
    {
      if (thisAudio.isVirtual || StartOfRound.Instance == null || StartOfRound.Instance.audioListener == null)
      {
        return;
      }

      // Handle reverb effects when inside
      UpdateReverbEffects();

      // Handle low pass filter
      UpdateLowPassFilter();

      // Check for occlusion periodically
      UpdateOcclusionCheck();
    }

    private bool CheckOcclusion()
    {
      if (GameNetworkManager.Instance == null || GameNetworkManager.Instance.localPlayerController == null)
      {
        return false;
      }

      PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;

      bool isPlayerOnShip = localPlayer.isInHangarShipRoom && !localPlayer.isInsideFactory;
      bool isSpectatedPlayerInFactory =
        localPlayer.isPlayerDead && localPlayer.spectatedPlayerScript != null && localPlayer.spectatedPlayerScript.isInsideFactory;

      // Check if player is in hangar ship room
      if (GameNetworkManager.Instance != null && GameNetworkManager.Instance.localPlayerController != null)
      {
        if (localPlayer.isInHangarShipRoom)
        {
          return true;
        }

        // If player is dead, check the spectated player
        if (localPlayer.isPlayerDead)
        {
          if (!isSpectatedPlayerInFactory)
          {
            return true;
          }
        }

        if (!isPlayerOnShip)
        {
          Vector3 playerPosition = StartOfRound.Instance.audioListener.transform.position;

          // Check if something is above the player
          if (Physics.Raycast(playerPosition, Vector3.up, out RaycastHit hit, 150f, 256, QueryTriggerInteraction.Ignore))
          {
            // If we hit something above the player, occlude
            return true;
          }
        }
      }

      // If not in hangar ship room, do the normal line of sight check
      if (StartOfRound.Instance != null && StartOfRound.Instance.audioListener != null)
      {
        Vector3 effectivePosition = GetEffectivePosition();
        return Physics.Linecast(effectivePosition, StartOfRound.Instance.audioListener.transform.position, 256, QueryTriggerInteraction.Ignore);
      }

      return false;
    }

    private void UpdateReverbEffects()
    {
      if (!useReverb || GameNetworkManager.Instance == null || GameNetworkManager.Instance.localPlayerController == null)
      {
        return;
      }

      bool isInside =
        GameNetworkManager.Instance.localPlayerController.isInsideFactory
        || (
          GameNetworkManager.Instance.localPlayerController.isPlayerDead
          && GameNetworkManager.Instance.localPlayerController.spectatedPlayerScript != null
          && GameNetworkManager.Instance.localPlayerController.spectatedPlayerScript.isInsideFactory
        );

      if (isInside)
      {
        Vector3 effectivePosition = GetEffectivePosition();
        float distanceToListener = Vector3.Distance(StartOfRound.Instance.audioListener.transform.position, effectivePosition);

        if (SoundManager.Instance.echoEnabled)
        {
          reverbFilter.dryLevel = Mathf.Lerp(
            reverbFilter.dryLevel,
            Mathf.Clamp(-(4.5f * (distanceToListener / (thisAudio.maxDistance / 12f))), -1270f, -1f),
            Time.deltaTime * 8f
          );
        }
        else
        {
          reverbFilter.dryLevel = Mathf.Lerp(
            reverbFilter.dryLevel,
            Mathf.Clamp(-(3.4f * (distanceToListener / (thisAudio.maxDistance / 5f))), -300f, -1f),
            Time.deltaTime * 8f
          );
        }
        reverbFilter.enabled = true;
      }
      else
      {
        reverbFilter.enabled = false;
      }
    }

    private void UpdateLowPassFilter()
    {
      if (overridingLowPass)
      {
        lowPassFilter.cutoffFrequency = lowPassOverride;
        return;
      }

      Vector3 effectivePosition = GetEffectivePosition();
      float distanceToListener = Vector3.Distance(StartOfRound.Instance.audioListener.transform.position, effectivePosition);

      if (occluded)
      {
        lowPassFilter.cutoffFrequency = Mathf.Lerp(
          lowPassFilter.cutoffFrequency,
          Mathf.Clamp(2500f / (distanceToListener / (thisAudio.maxDistance / 2f)), 900f, 4000f),
          Time.deltaTime * 8f
        );
      }
      else
      {
        lowPassFilter.cutoffFrequency = Mathf.Lerp(lowPassFilter.cutoffFrequency, 10000f, Time.deltaTime * 8f);
      }
    }

    private void UpdateOcclusionCheck()
    {
      checkInterval += Time.deltaTime;

      if (checkInterval >= 0.25f)
      {
        checkInterval = UnityEngine.Random.Range(0f, 0.3f);

        // Update occlusion state
        occluded = CheckOcclusion();
      }
    }
  }
}
