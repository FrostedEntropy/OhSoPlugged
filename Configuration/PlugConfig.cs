using BepInEx;
using BepInEx.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;

namespace OhSoPlugged
{
    public class PlugConfig
    {
        /*
         * The SexConfig.json files contains customizable configuration for the toys. 
         * It is better to modify the values of the SexConfig.json file than having to rebuild
         * the code every time a minor value has to be changed. 
         */
        public bool buttplugEnabled = false;
        public float vibrationAlwaysOnStrength = 0.0f;
        public float maxButtplugStrength;
        public float refreshRate = 0.5f;
        public float postSexHealthRegen = 1;
        public bool logAnimationNames = false;
        public Config config;
        public Dictionary<string, Dictionary<string, Dictionary<string, float>>> sexScenesConfig;

        static PlugConfig()
        {
            Config config = new Config();
        }

        public void GetConfigsFromFile()
        {
            string jsonFilePath = @"BepInEx\\plugins\\SexConfig.json";
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, jsonFilePath);
            using (FileStream stream = File.OpenRead(fullPath))
            {
                string text = File.ReadAllText(fullPath);
                config = JsonConvert.DeserializeObject<Config>(text);
            }
        }

        public void SetConfigVariables()
        {
            try
            {
                //Plugin Status - 0:disabled, 1:enabled
                OSHPatcher.logger.LogInfo("Checking if plugin is enabled");
                if(config.toysConfig["ButtplugEnabled"] == 1) 
                {
                    OSHPatcher.logger.LogInfo("Plugin enabled. Proceeding...");
                    buttplugEnabled = true;
                }
                else
                {
                    return;
                }

                //Vibration Always On - 0:disabled, 1:enabled
                OSHPatcher.logger.LogInfo("Checking if toy should continue to vibrate out of scene");
                if (config.toysConfig["VibrationAlwaysOn"] == 1)
                {
                    OSHPatcher.logger.LogInfo("Vibration Always On enabled.");
                    vibrationAlwaysOnStrength = config.toysConfig["VibrationAlwaysOnStrength"] > 1.0f ? 1.0f : config.toysConfig["VibrationAlwaysOnStrength"];
                }
                else
                {
                    return;
                }

                //Toy Strength Multiplier. Float 0.0 - 1.0
                OSHPatcher.logger.LogInfo("Fetching toy strength...");
                maxButtplugStrength = config.toysConfig["MaxButtplugStrength"];
                if(maxButtplugStrength > 1)
                {
                    maxButtplugStrength = 1;
                }
                else if(maxButtplugStrength <= 0) //0 is the same as disabling the toys, but patching will still occur. Not advised to use. 
                {
                    OSHPatcher.logger.LogWarning("MaxButtplugStrength was set to 0 or less. Toys will not activate, but code would still execute. Disabling plugin...");
                    buttplugEnabled = false;
                    return;
                }

                //Refresh Rate Float. Default value is 0.5
                OSHPatcher.logger.LogInfo("Fetching refresh rate for sex scenes...");
                refreshRate = config.toysConfig["RefreshRate"];
                if (config.toysConfig["RefreshRate"] <= 0) 
                {
                    OSHPatcher.logger.LogWarning("RefreshRate was set to 0 or less, which may cause various issues during animations. Disabling plugin...");
                    OSHPatcher.logger.LogWarning("Many scenes will have extra miliseconds, like 4.5 seconds, so using a round number is not recommended.");
                    OSHPatcher.logger.LogWarning("Consider using a refresh rate between 0.1 and 1. Default value is 0.5");
                    buttplugEnabled = false;
                    return;
                }

                //Health - Float. Acts as a multiplier for the health regen after a sex scene ends. 
                OSHPatcher.logger.LogInfo("Fetching Health Regen preference.");
                postSexHealthRegen = config.toysConfig["PostSexHealthRegen"];
                if( postSexHealthRegen <= 0)
                {
                    OSHPatcher.logger.LogInfo("PostSexHealthRegen was set to 0 or less. No health will be restored after sex. (Default: 1).");
                    postSexHealthRegen = 0;
                }


                //Logging Animation Enabled. Integer (treated as a float) that is translated to bool
                try
                {
                    if (config.toysConfig["LogAnimationNames"] == 1)
                    {
                        OSHPatcher.logger.LogInfo("Animation names will be logged.");
                        logAnimationNames = true;
                    }
                    else if(config.toysConfig["LogAnimationNames"] == 0)
                    {
                        OSHPatcher.logger.LogInfo("Animation names will not be logged.");
                    }
                    else
                    {
                        throw (new Exception());
                    }
                }
                catch
                {
                    OSHPatcher.logger.LogInfo("Invalid value entered for LogAnimationNames. Only 0 or 1 is accepted. Using the default 0 value (disabled)");
                }

                /* Capturing Sex Scenes data. You can add support for new scenes added in future game updates without rebuilding the code.
                 * I recommend modifying the file in a text editor like Notepad++ where row numbers are displayed as this will make troubleshooting
                 * easier if anything gets messed up. 
                 * 
                 * The data is structured as follows:
                 * 1. Animation name - These are manually pulled from the logs generated by the isSexScene method
                 * of the SexScenesConfig.cs file. Set LoganimationNames to 1 to log them.  
                 * Animations are ordered as follows: NPCs, Monsters, Multi-Monster scenes. 
                 * Each section ends with its own set of templates that you can copy and paste, but avoid launching the
                 * game with duplicated templates as this will cause errors. You can CTRL+F "TEMPLATE" to find them more easily.   
                 * 
                 * 2. Scenes# where # is a number index starting at 1. There is no cap, so you could have as many scenes as you want.
                 * If desired, you could split a single scene into multiples to match each movement on screen,
                 * but you'll probably want to adjust the RefreshRate accordingly for this to work more accurately (Default is 0.5).
                 * 
                 * 3. Strength and Duration. Both are floats. 
                 * Strength is an number from 0 to 1, where 0 is off, and 1 is the maximum speed. 
                 * Duration is self-explanatory. Not all scenes end exactly on an round number of on a .5. If you want to be more accurate,
                 * then you may need to adjust the RefreshRate accordingly. 
                 * A note on duration: I used a timer app on my phone where each new scene was counted as a lap. This might help you if you're
                 * manually adding new scenes. 
                 * 
                 * Warning: Make sure to maintain the formatting here as any errors will cause the script to fail. 
                 */
                OSHPatcher.logger.LogInfo("Fetching Sex Scenes...");
                sexScenesConfig = config.sexScenesConfig;  
            }
            catch(Exception e)
            {
                OSHPatcher.logger.LogInfo($"Error: {e}");
            }
        }
    }

    public class Config
    {
        public Dictionary<string, float> toysConfig { get; set; }
        public Dictionary<string, Dictionary<string, Dictionary<string, float>>> sexScenesConfig { get; set; }
    }
}
