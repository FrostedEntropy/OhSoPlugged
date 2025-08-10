using _0G;
using HarmonyLib;
using OhSoPlugged.Configuration;
using OSH;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OhSoPlugged.Patches
{
    [HarmonyLib.HarmonyPatch(typeof(GraphicController))]
    internal class GraphicControllerPatch
    {
        //Patching GraphicController as it's the easiest method I found to capture which animation we are working with to
        //determine if we want to trigger the toys or not. 
        [HarmonyPatch("SetAnimation")]
        [HarmonyPatch(new Type[] { typeof(AnimationContext), typeof(String), typeof(GraphicController.AnimationEndHandler) })]
        [HarmonyPrefix]
        static async void TriggerOnSexanimation(string animationName)
        {
            try
            {
                if (!OSHPatcher.buttplugManager.IsConnected())
                {
                    throw (new Exception("Initiface Central is Offline"));
                }
            }
            catch (Exception error)
            {
                OSHPatcher.logger.LogWarning($"{DateTime.UtcNow} {error}. Skipping toy interactions.");
                return;
            }
            if (OSHPatcher.sexScenesConfig.isSexScene(animationName))
            {
                float intensity = (float)OSHPatcher.plugConfig.maxButtplugStrength;
                CancellationToken _token = CancellationToken.None;

                OSHPatcher.logger.LogInfo($"{DateTime.UtcNow} Animation Name = {animationName}");

                //Currently, up to 3 scenes can occur in a row, forcing us to cancel
                //previous scenes to use the correct scene instructions. Scene changes can occur
                //when the player is grabbed while masturbating, or grabbed by a 2nd enemy while
                //already engaging in sex with another enemy. 
                if (OSHPatcher.cancellationTokensList.Count > 0)
                {
                    OSHPatcher.logger.LogInfo($"{DateTime.UtcNow} Cancelling Source...");
                    try
                    {
                        OSHPatcher.cancellationTokensList[0].Cancel();
                        await Task.WhenAll();
                        OSHPatcher.cancellationTokensList[0].Dispose();
                        OSHPatcher.cancellationTokensList.RemoveAt(0);
                    }
                    catch{ }
                    
                }
                else
                {
                    CancellationTokenSource _cancelSource = new CancellationTokenSource();
                    _token = _cancelSource.Token;
                    OSHPatcher.cancellationTokensList.Add(_cancelSource);
                }
                OSHPatcher.logger.LogInfo($"{DateTime.UtcNow} Triggering Toy...");
                OSHPatcher.sexScenesConfig.SetSexScenesStates(animationName);
                await OSHPatcher.buttplugManager.TriggerToysAsync(OSHPatcher.sexScenesConfig.currentSexScene, OSHPatcher.sexScenesConfig.sexAnimationConfig, intensity, _token);

                //HP is lost during sex scenes. Since script doesn't support scene cancellation yet, we regenerate a portion of the HP lost 
                //to make up for this limitation. This allows the player to enjoy the full scene experience, without as much of the downsides. 
                try {
                    OSHPatcher.sexScenesConfig.damageTaker.HP += (int)OSHPatcher.sexScenesConfig.damageToRegen / 2 * OSHPatcher.plugConfig.postSexHealthRegen;
                    OSHPatcher.sexScenesConfig.damageToRegen = 0;
                }
                catch
                {
                    return;
                }
                
            }
        }

        
    }

    [HarmonyLib.HarmonyPatch(typeof(OhSoGraphicController))]
    internal class StopToysWhenSexEndsPatch
    {
        [HarmonyPatch("StopSex")]
        [HarmonyPatch(new Type[] { typeof(GraphicController), typeof(bool) })]
        [HarmonyPostfix]
        static async void StopSexToys()
        {
            OSHPatcher.logger.LogInfo("Checking if Sex is ending...");
            if (OSHPatcher.sexScenesConfig.currentSexScene != null)
            {
                OSHPatcher.logger.LogInfo("Sex Ending");
                try
                {
                    OSHPatcher.cancellationTokensList[0].Cancel();
                    await Task.WhenAll();
                    OSHPatcher.cancellationTokensList[0].Dispose();
                    OSHPatcher.cancellationTokensList.RemoveAt(0);
                }
                catch { }
            }
        }
    }


}
