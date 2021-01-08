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
        private Dictionary<string, HudText> texts = new Dictionary<string, HudText>();

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
            foreach(HudText timer in texts.Values)
            {
                if (timer.Update())
                    toRemove.Add(timer.Id);
            }

            foreach (string key in toRemove)
                texts.Remove(key);

        }

        private void OnHudReady()
        {
            MyAPIGateway.Utilities.RegisterMessageHandler(TextAPI.MessageId, ReceiveData);
            MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(TextAPI.PacketId, ReceiveData);

            StringBuilder sb = new StringBuilder();
            List<MyStringId> ids = new List<MyStringId>();
            HudAPIv2.APIinfo.GetFonts(ids);
            foreach(var id in ids)
                sb.Append(' ').Append(id).Append(',');
            MyAPIGateway.Utilities.ShowNotification(sb.ToString(), 30000);
            TextAPI.Text test = new TextAPI.Text("test", new TimeSpan(0, 0, 45), "<color=red>Some Text", Vector2D.Zero, 2, TextAPI.TextAlignment.Right, "FreeMono_Racing");
            test.SendToAll();
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
            TextAPI.Text obj = MyAPIGateway.Utilities.SerializeFromBinary<TextAPI.Text>(data);
            if(obj != null)
            {
                HudText temp;
                if (texts.TryGetValue(obj.id, out temp))
                    temp.Delete();
                if (!string.IsNullOrWhiteSpace(obj.text))
                    texts[obj.id] = new HudText(obj.id, hud, obj.Length, obj.text, obj.Center, obj.scale, obj.Alignment, obj.font);
            }
        }
    }
}