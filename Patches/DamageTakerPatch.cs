using _0G;
using Buttplug.Client;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OhSoPlugged.Patches
{
    [HarmonyLib.HarmonyPatch(typeof(DamageTaker))]
    internal class DamageTakerPatch
    {
        [HarmonyPatch("Damage")]
        [HarmonyPatch(new Type[] { typeof(Attack), typeof(Vector3), typeof(Vector3) })]
        [HarmonyPrefix]
        static void VibrateOnDamage1(Attack attack)
        {
            //Regular Attack
            if (!attack.Attacker.Body.IsPlayerCharacter && OSHPatcher.sexScenesConfig.currentSexScene == null)
            {
                OSHPatcher.buttplugManager.connectedDevices.ForEach(Action);
            }
        }
        
        [HarmonyPatch("Damage")]
        [HarmonyPatch(new Type[] {typeof(Attacker), typeof(AttackAbility), typeof(Vector3), typeof(Vector3)})]
        [HarmonyPrefix]
        static void VibrateOnDamage2(Attacker attacker)
        {
            //Special Charm Attack
            if (!attacker.Body.IsPlayerCharacter && OSHPatcher.sexScenesConfig.currentSexScene == null) {
                OSHPatcher.buttplugManager.connectedDevices.ForEach(Action);
            }
            
        }
        
        static async void Action(ButtplugClientDevice device)
        {
            await device.VibrateAsync(1.0f);
            await Task.Delay((1000));
            await device.VibrateAsync(OSHPatcher.plugConfig.vibrationAlwaysOnStrength);
        }
    }
}
