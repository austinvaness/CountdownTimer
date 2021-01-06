using Draygo.API;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Utils;
using VRageMath;
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;

namespace avaness.CountdownTimer
{
    public class HudTimer : IEquatable<HudTimer>
    {
        private readonly HudAPIv2 hud;
        private HudAPIv2.HUDMessage hudText;
        private Vector2D center;
        private readonly StringBuilder msg = new StringBuilder();
        private readonly string format;
        private readonly string text;
        private readonly TimeSpan length;
        private TimeSpan current;
        private readonly TimeSpan oneTick = new TimeSpan((long)(MyEngineConstants.UPDATE_STEP_SIZE_IN_SECONDS * TimeSpan.TicksPerSecond));
        private readonly TimeSpan zero = TimeSpan.Zero;
        private readonly bool down;
        private readonly double scale;
        public string Id { get; }

        public HudTimer(string id, HudAPIv2 hud, TimeSpan length, string text, string timerFormat, Vector2D center, double scale, bool down = true)
        {
            this.Id = id;
            this.text = text;
            if (!text.Contains("{0}"))
                text = "{0}";
            this.hud = hud;
            format = timerFormat;

            try
            {
                string test = zero.ToString(format);
            }
            catch (FormatException)
            {
                MyLog.Default.WriteLineAndConsole($"Error: {timerFormat} is not a valid TimeSpan format.");
                format = "g";
            }

            this.length = length;
            this.center = center;
            this.down = down;
            this.scale = scale;
            if (down)
                current = length;
            else
                current = zero;

            msg.Clear().AppendFormat(timerFormat, length);

            if (hud.Heartbeat)
                Create();

        }

        /// <summary>
        /// Update the timer.
        /// </summary>
        /// <returns><see langword="true"/> if the timer is still running.</returns>
        public bool Update()
        {
            if(down)
            {
                current -= oneTick;
                if (current.Ticks < 0)
                    current = zero;
            }
            else
            {
                current += oneTick;
                if (current.Ticks > length.Ticks)
                    current = length;
            }

            msg.Clear().AppendFormat(text, current.ToString(format));

            if((down && current.Ticks == 0) || (!down && current.Ticks == length.Ticks))
            {
                Delete();
                return false;
            }
            else if(hudText == null)
            {
                if (hud.Heartbeat)
                    Create();
            }
            else
            {
                Center();
            }
            return true;
        }

        public void Delete()
        {
            if(hudText != null)
                hudText.DeleteMessage();
        }

        private void Create()
        {
            hudText = new HudAPIv2.HUDMessage(msg, center, Scale: scale, Blend: BlendTypeEnum.PostPP);
            Center();
        }

        private void Center()
        {
            Vector2D size = hudText.GetTextLength();
            hudText.Offset = size / -2;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as HudTimer);
        }

        public bool Equals(HudTimer other)
        {
            return other != null &&
                   Id == other.Id;
        }

        public override int GetHashCode()
        {
            return 1877310944 + EqualityComparer<string>.Default.GetHashCode(Id);
        }

        public static bool operator ==(HudTimer left, HudTimer right)
        {
            return EqualityComparer<HudTimer>.Default.Equals(left, right);
        }

        public static bool operator !=(HudTimer left, HudTimer right)
        {
            return !(left == right);
        }
    }
}