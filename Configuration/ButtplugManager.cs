using Buttplug;
using Buttplug.Client;
using Buttplug.Core;
using OhSoPlugged;
using System;
using System.Data;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using OhSoPlugged.Configuration;
using System.Threading;
using System.Linq;

namespace OhSoPlugged
{
    public class ButtplugManager 
    {
        public List<ButtplugClientDevice> connectedDevices { get; set; }

        private ButtplugClient buttplugClient { get; set; }

        public ButtplugManager(string clientName)
        {
            this.connectedDevices = new List<ButtplugClientDevice>();
            this.buttplugClient = new ButtplugClient(clientName);
            OSHPatcher.logger.LogInfo("Buttplug Client created for " + clientName);
            this.buttplugClient.DeviceAdded += this.HandleDeviceAdded;
            this.buttplugClient.DeviceRemoved += this.HandleDeviceRemoved;          
        }

        public bool IsConnected()
        {
            return this.buttplugClient.Connected;
        }

        public async void ConnectDevices()
        {
            if (buttplugClient.Connected) { return; }

            try
            {
                var connector = new ButtplugWebsocketConnector(new Uri("ws://127.0.0.1:12345"));
                OSHPatcher.logger.LogInfo("Attempting to connect to Initface server...");
                await buttplugClient.ConnectAsync(connector);
                OSHPatcher.logger.LogInfo("Connection success. Scanning for devices...");
                await buttplugClient.StartScanningAsync();
            }
            catch (ButtplugException exception)
            {
                throw(exception);
            }
        }

        public void VibrateConnectedDevices(double intensity)
        {
            intensity += 0f;

            async void Action(ButtplugClientDevice device)
            {
                await device.VibrateAsync(Mathf.Clamp((float)intensity, 0f, 1.0f));
            }

            connectedDevices.ForEach(Action);
        }

        public void StopConnectedDevices()
        {
            connectedDevices.ForEach(async (ButtplugClientDevice device) => await device.Stop());
        }

        internal void CleanUp()
        {
            this.StopConnectedDevices();
        }
       
        
        private void HandleDeviceAdded(object sender, DeviceAddedEventArgs args)
        {
            if (!this.IsVibratableDevice(args.Device)) //&& (!this.IsLinearDevice(args.Device) && !this.IsOscillateDevice(args.Device)))
            {
                OSHPatcher.logger.LogInfo(args.Device.Name + " was detected but ignored due to it not being compatible.");
                return;
            }
            OSHPatcher.logger.LogInfo(args.Device.Name + " connected to client " + this.buttplugClient.Name);
            this.connectedDevices.Add(args.Device);
        }

        private void HandleDeviceRemoved(object sender, DeviceRemovedEventArgs args)
        {
            if (!this.IsVibratableDevice(args.Device)) //&& (!this.IsLinearDevice(args.Device) && !this.IsOscillateDevice(args.Device)))
            {
                return;
            }
            OSHPatcher.logger.LogInfo(args.Device.Name + " disconnected from client " + this.buttplugClient.Name);
            this.connectedDevices.Remove(args.Device);
        }

        private bool IsVibratableDevice(ButtplugClientDevice device)
        {
            return device.VibrateAttributes.Count > 0;
        }

        //Can't figure out how to properly trigger toys like Lovense Solace Pro yet, but leaving this here for future reference
        /*
        private bool IsLinearDevice(ButtplugClientDevice device)
        {
            return device.LinearAttributes.Count > 0;
        }

        private bool IsOscillateDevice(ButtplugClientDevice device)
        {
            return device.OscillateAttributes.Count > 0;
        }*/

        public async Task TriggerToysAsync(string currentSexScene, List<Dictionary<string, float>> toyDetailArray, float intensity, CancellationToken token)
        {
            float countdownRefresh = OSHPatcher.plugConfig.refreshRate;
            OSHPatcher.logger.LogInfo($"{DateTime.UtcNow} Animation Name = {currentSexScene}");

            //Setting some variables we need to deal with scenes being cancelled when an enemy grabs the player during
            //an existing scene. 
            OSHPatcher.sexScenesConfig.timesUniqueSceneTriggered++;
            OSHPatcher.sexScenesConfig.totalUniqueScenesIDsList.Add(OSHPatcher.sexScenesConfig.timesUniqueSceneTriggered);
            int uniqueSexSceneID = OSHPatcher.sexScenesConfig.timesUniqueSceneTriggered;
 
            try
            {
                for (int i = 0; i < toyDetailArray.ToArray().Length; i++)
                {
                    if (token.IsCancellationRequested)
                    {
                        throw (new OperationCanceledException());
                    }

                    if (currentSexScene != OSHPatcher.sexScenesConfig.oldSexScene && OSHPatcher.sexScenesConfig.sceneDuration.Count > 0)
                    {
                        OSHPatcher.logger.LogInfo($"{DateTime.UtcNow} INDEX: {i}   ToyDetail: {toyDetailArray[i]["Duration"]} CurrentScene: {currentSexScene}");
                        //Fix for Swoon-->Single-->Multi
                        if (OSHPatcher.sexScenesConfig.timesUniqueSceneTriggered == 3)
                        {
                            toyDetailArray = OSHPatcher.sexScenesConfig.setSexSceneList(OSHPatcher.sexScenesConfig.currentSexScene);
                            OSHPatcher.sexScenesConfig.timesUniqueSceneTriggered++;
                        }

                        //Minor time adjustments fix for when the player is grabbed during an existing sex scene. 
                        float durationDifference = toyDetailArray[i]["Duration"] - Math.Abs((float)toyDetailArray[i]["Duration"] - (float)OSHPatcher.sexScenesConfig.sceneDuration[0]);
                        if (durationDifference > 0 && OSHPatcher.sexScenesConfig.timesUniqueSceneTriggered < 2)
                        {
                            toyDetailArray[i]["Duration"] -= durationDifference;

                        }
                        else if (durationDifference < 0)
                        {
                            toyDetailArray[i]["Duration"] += countdownRefresh;
                        }
                        OSHPatcher.sexScenesConfig.oldSexScene = currentSexScene;
                    }
                    OSHPatcher.sexScenesConfig.sceneDuration.Add(toyDetailArray[i]["Duration"]);

                    //Activating toy for the duration of the scene. We split it in a for loop to allow us to cancel 
                    //the task mid-scene in case the player is grabbed during sex else it would continue on and
                    //interfere with the new scene data. 
                    for (float j = 0; j < toyDetailArray[i]["Duration"]; j += countdownRefresh)
                    {
                        if(OSHPatcher.sexScenesConfig.totalUniqueScenesIDsList.Count >= 3 && uniqueSexSceneID == 2)
                        {
                            return;
                        }
                        async void Action(ButtplugClientDevice device)
                        {
                            if (this.IsVibratableDevice(device))
                            {
                                OSHPatcher.logger.LogInfo($"{DateTime.UtcNow} Vibration Strength: {toyDetailArray[i]["Strength"]} - Duration: {toyDetailArray[i]["Duration"]} ");
                                await device.VibrateAsync(Mathf.Clamp(toyDetailArray[i]["Strength"] * intensity, 0f, 1.0f));
                            }
                        }
                        connectedDevices.ForEach(Action);
                        await Task.WhenAll();
                        OSHPatcher.logger.LogInfo($"{DateTime.UtcNow} Scene Time Left: {OSHPatcher.sexScenesConfig.sceneDuration[OSHPatcher.sexScenesConfig.sceneDuration.Count() - 1]}");
                            
                            

                        if (!token.IsCancellationRequested)
                        {
                            await Task.Delay((int)(countdownRefresh * 1000));
                        }
                        else
                        {
                            throw (new OperationCanceledException());
                        }

                        if (OSHPatcher.sexScenesConfig.sceneDuration.Count() > 0)
                        {
                            OSHPatcher.sexScenesConfig.sceneDuration[OSHPatcher.sexScenesConfig.sceneDuration.Count() - 1] -= countdownRefresh;
                        }
                        else
                        {
                            OSHPatcher.sexScenesConfig.sceneDuration[0] -= countdownRefresh;
                        }
                        OSHPatcher.sexScenesConfig.damageToRegen += countdownRefresh;
                    }
                }

                //Having the toy suddenly stop is unpleasant, so I added a quick ramp down to
                //smooth the ending of the animation. 
                async void RampDown(ButtplugClientDevice device)
                {
                    if (token.IsCancellationRequested)
                    {
                        throw (new OperationCanceledException());
                    }
                    if (this.IsVibratableDevice(device))
                    {
                        await device.VibrateAsync(Mathf.Clamp(0.5f * intensity, 0f, 1.0f));
                        await Task.Delay((int)(1 * 1000f));
                        await device.VibrateAsync(Mathf.Clamp(0.2f * intensity, 0f, 1.0f));
                        await Task.Delay((int)(1 * 1000f));
                        await device.VibrateAsync(OSHPatcher.plugConfig.vibrationAlwaysOnStrength);
                    }
                }

                OSHPatcher.logger.LogInfo($"{DateTime.UtcNow} Scene over. Ramping down toy(s)...");
                connectedDevices.ForEach(RampDown);
                OSHPatcher.sexScenesConfig.sceneDuration.Clear();
                OSHPatcher.sexScenesConfig.currentSexScene = null;
                OSHPatcher.sexScenesConfig.oldSexScene = null;
                OSHPatcher.sexScenesConfig.timesUniqueSceneTriggered = 0;
                OSHPatcher.cancellationTokensList.Clear();
                OSHPatcher.sexScenesConfig.totalUniqueScenesIDsList.Clear();
            }
            catch (OperationCanceledException)
            {
                await TurnOffToyEffectsAsync();
                OSHPatcher.logger.LogInfo($"{DateTime.UtcNow} Function Cancelled");
            }
        }

        public async Task TurnOffToyEffectsAsync()
        {
            OSHPatcher.logger.LogInfo($"{DateTime.UtcNow} Ramping Down");
            async void Action(ButtplugClientDevice device)
            {
                await device.VibrateAsync(OSHPatcher.plugConfig.vibrationAlwaysOnStrength);
            }
            connectedDevices.ForEach(Action);
            await Task.WhenAll();
            
        }
    }
}
