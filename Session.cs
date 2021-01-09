using avaness.ServerTextAPI.API;
using Draygo.API;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game.Components;
using VRage.Utils;
using VRageMath;

namespace avaness.ServerTextAPI
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class CountdownTimerSession : MySessionComponentBase
    {
        private bool init = false;
        private HudAPIv2 hud;
        private readonly Dictionary<string, HudText> texts = new Dictionary<string, HudText>();

        protected override void UnloadData()
        {
            MyAPIGateway.Utilities.UnregisterMessageHandler(TextAPI.MessageId, ReceiveData);
            MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(TextAPI.PacketId, ReceiveData);
            hud?.Unload();
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
            foreach(HudText text in texts.Values)
            {
                if (text.Update())
                    toRemove.Add(text.Id);
            }

            foreach (string key in toRemove)
                texts.Remove(key);

        }

        private void OnHudReady()
        {
            MyAPIGateway.Utilities.RegisterMessageHandler(TextAPI.MessageId, ReceiveData);
            MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(TextAPI.PacketId, ReceiveData);
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
            try
            {
                TextAPI.Text obj = MyAPIGateway.Utilities.SerializeFromBinary<TextAPI.Text>(data);
                if (obj != null)
                {
                    HudText temp;
                    if (texts.TryGetValue(obj.id, out temp))
                        temp.Delete();
                    if (!string.IsNullOrWhiteSpace(obj.text))
                        texts[obj.id] = new HudText(obj.id, hud, obj.Length, obj.text, obj.Center, obj.scale, obj.Alignment, obj.font);
                }
            }
            catch { }
        }
    }
}