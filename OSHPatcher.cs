using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using OhSoPlugged.Configuration;
using OhSoPlugged.Patches;
using System.Collections.Generic;
using System.Threading;

namespace OhSoPlugged
{
    [BepInPlugin("FrostedEntropy.OhSoPlugged", "OhSoPlugged", "1.0.0")]
    public class OSHPatcher : BaseUnityPlugin
    {
        internal static ButtplugManager buttplugManager { get; set; }
        internal static ManualLogSource logger { get; set; }
        internal static PlugConfig plugConfig { get; set; }
        internal static SexScenesConfig sexScenesConfig { get; set; }
        internal static List<CancellationTokenSource> cancellationTokensList { get; set; }
        internal static CancellationTokenSource _cancelSource { get; set; }
        internal static CancellationToken _token {  get; set; }

        private void Awake()
        {
            logger = BepInEx.Logging.Logger.CreateLogSource("OhSoPlugged");
            logger.LogInfo("Initializing OhSoPlugged");
            Instance = this;
            
            plugConfig = new PlugConfig();
            plugConfig.GetConfigsFromFile();
            plugConfig.SetConfigVariables();
            if (plugConfig.buttplugEnabled)
            {
                OSHPatcher.logger.LogInfo("Patching Game...");
                cancellationTokensList = new List<CancellationTokenSource>();
                sexScenesConfig = new SexScenesConfig();
                
                try 
                {
                    buttplugManager = new ButtplugManager("OhSoPlugged");
                    buttplugManager.ConnectDevices();
                    Harmony harmony = new Harmony("FrostedEntropy.OhSoPlugged");
                    harmony.PatchAll(typeof(DamageTakerPatch));
                    harmony.PatchAll(typeof(SexPatch));
                    harmony.PatchAll(typeof(GraphicControllerPatch));
                    harmony.PatchAll(typeof(StopToysWhenSexEndsPatch));
                }
                catch
                {
                    logger.LogWarning("Initface Central is offline. If you wish to use toy interactions, please enable Initface Central and restart the game.");
                }
                
            }
            else
            {
                OSHPatcher.logger.LogWarning("Plugin is disabled. You can set ToysConfig.ButtplugEnabled in the SexConfig.json file to 1 to enable them.");
            }
            
        }
        public static OSHPatcher Instance;
    }
}
