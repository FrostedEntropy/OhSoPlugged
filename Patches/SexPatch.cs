using Buttplug.Client;
using HarmonyLib;
using OSH;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OhSoPlugged.Patches
{
    [HarmonyLib.HarmonyPatch(typeof(Sex))]
    internal class SexPatch
    {
        [HarmonyPatch("Init")]
        [HarmonyPrefix]
        static void GetPlayerCharacter(OhSoCharacter initiator, OhSoCharacter participant)
        {
            //Best way I found to catch the player data. We only use this to determine
            //which actor to heal after a sex scene. 
            if (participant.IsPlayerCharacter)
            {
                OSHPatcher.sexScenesConfig.damageTaker = participant.DamageTaker;
            }
            else
            {
                OSHPatcher.sexScenesConfig.damageTaker = initiator.DamageTaker;
            }
        }
    }
}
