using BepInEx;
using OSH;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OhSoPlugged.Configuration
{
    internal class SexScenesConfig
    {
        public SexScenesConfig() { }
        public string currentSexScene = null;
        public string oldSexScene = null;
        public List<float> sceneDuration = new List<float>();
        public int timesUniqueSceneTriggered = 0;
        public List<int> totalUniqueScenesIDsList = new List<int>();
        public List<Dictionary<string, float>> sexAnimationConfig { get; set; }
        public OhSoDamageTaker damageTaker = null;
        public float damageToRegen = 0;

        public bool isSexScene(string sceneName)
        {
            if (OSHPatcher.plugConfig.logAnimationNames)
            {
                OSHPatcher.logger.LogInfo($"{DateTime.UtcNow} CurrentScene: {sceneName}");
            }

            if (OSHPatcher.plugConfig.sexScenesConfig.ContainsKey(sceneName))
            {
                return true;
            }
            return false;
        }

        public void SetSexScenesStates(string sceneName) 
        {
            //We keep track of the previous scene to help manage issues when an enemy grabs 
            //the player during an existing sex scene. 
            OSHPatcher.logger.LogInfo(sceneName);
            if (oldSexScene == null)
            {
                oldSexScene = sceneName;
            }
            else
            {
                oldSexScene = currentSexScene;
            } 
            currentSexScene = sceneName;
            this.sexAnimationConfig = setSexSceneList(sceneName);
        }

        public List<Dictionary<string,float>> setSexSceneList(string sceneName)
        {
            List < Dictionary<string, float> > list = new List < Dictionary<string, float> >();
            for (int i = 1; 1 <= OSHPatcher.plugConfig.sexScenesConfig[sceneName].Count(); i++)
            {
                if (OSHPatcher.plugConfig.sexScenesConfig[sceneName].ContainsKey($"Scene{i}")){
                    list.Add(OSHPatcher.plugConfig.sexScenesConfig[sceneName][$"Scene{i}"]);
                }
                else
                {
                    return list;
                }
            }
            throw (new ArgumentException("No corresponding scenes found. Closing."));
        }    
    }
}
