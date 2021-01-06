using avaness.CountdownTimer.API;
using Draygo.API;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game.Components;
using VRageMath;

namespace avaness.CountdownTimer
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class CountdownTimerSession : MySessionComponentBase
    {
        private bool init = false;
        private HudAPIv2 hud;
        private Dictionary<string, HudTimer> timers = new Dictionary<string, HudTimer>();

        protected override void UnloadData()
        {
            MyAPIGateway.Utilities.UnregisterMessageHandler(TimerAPI.MessageId, ReceiveData);
            MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(TimerAPI.PacketId, ReceiveData);
            foreach (HudTimer timer in timers.Values)
                timer.Delete();
        }

        private void Start()
        {
            hud = new HudAPIv2(OnHudReady);
            init = true;
        }

        public override void UpdateAfterSimulation()
        {
            if (MyAPIGateway.Session == null)
                return;
            if (!init)
                Start();

            List<string> toRemove = new List<string>();
            foreach(HudTimer timer in timers.Values)
            {
                if (!timer.Update())
                    toRemove.Add(timer.Id);
            }

            foreach (string key in toRemove)
                timers.Remove(key);

        }

        private void OnHudReady()
        {
            MyAPIGateway.Utilities.RegisterMessageHandler(TimerAPI.MessageId, ReceiveData);
            MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(TimerAPI.PacketId, ReceiveData);
        }

        private void ReceiveData(ushort packetId, byte[] data, ulong sender, bool fromServer)
        {
            DeserializeData(data);
        }

        private void ReceiveData(object obj)
        {
            byte[] data = obj as byte[];
            if (data != null)
                DeserializeData(data);
        }

        private void DeserializeData(byte[] data)
        {
            TimerAPI.Timer obj = MyAPIGateway.Utilities.SerializeFromBinary<TimerAPI.Timer>(data);
            if(obj != null)
            {
                HudTimer temp;
                if (timers.TryGetValue(obj.id, out temp))
                    temp.Delete();
                timers[obj.id] = new HudTimer(obj.id, hud, obj.length, obj.text, obj.timerFormat, obj.center, obj.scale, obj.down);
            }
        }
    }
}