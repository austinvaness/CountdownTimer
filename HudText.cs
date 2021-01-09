using Draygo.API;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game;
using VRageMath;
using avaness.ServerTextAPI.API;
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;

namespace avaness.ServerTextAPI
{
    public class HudText : IEquatable<HudText>
    {
        public string Id { get; }

        private HudAPIv2.HUDMessage hudText;
        private TimeSpan length;
        private readonly TimeSpan oneTick = new TimeSpan((long)(MyEngineConstants.UPDATE_STEP_SIZE_IN_SECONDS * TimeSpan.TicksPerSecond));

        public HudText(string id, TimeSpan length, string text, Vector2D center, double scale, TextAPI.TextAlignment alignment, string font)
        {
            Id = id;
            this.length = length;
            int ttl = (int)(length.TotalSeconds / MyEngineConstants.UPDATE_STEP_SIZE_IN_SECONDS);
            hudText = new HudAPIv2.HUDMessage(new StringBuilder(text), center, TimeToLive: ttl, Scale: scale, Blend: BlendTypeEnum.PostPP, Font: font);
            Align(alignment);
        }

        /// <summary>
        /// Update the text.
        /// </summary>
        /// <returns><see langword="true"/> if the timer is still running.</returns>
        public bool Update()
        {
            if (hudText == null)
                return false;

            length -= oneTick;
            return length.Ticks > 0;
        }

        public void Delete()
        {
            if(hudText != null)
            {
                hudText.DeleteMessage();
                hudText = null;
            }
        }

        private void Align(TextAPI.TextAlignment alignment)
        {
            Vector2D size = hudText.GetTextLength();
            Vector2D newOffset = new Vector2D();
            switch (alignment)
            {
                case TextAPI.TextAlignment.Left:
                    break;
                case TextAPI.TextAlignment.Center:
                    newOffset.X = size.X / -2;
                    break;
                case TextAPI.TextAlignment.Right:
                    newOffset.X = -size.X;
                    break;
            }
            hudText.Offset = newOffset;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as HudText);
        }

        public bool Equals(HudText other)
        {
            return other != null &&
                   Id == other.Id;
        }

        public override int GetHashCode()
        {
            return 1877310944 + EqualityComparer<string>.Default.GetHashCode(Id);
        }

        public static bool operator ==(HudText left, HudText right)
        {
            return EqualityComparer<HudText>.Default.Equals(left, right);
        }

        public static bool operator !=(HudText left, HudText right)
        {
            return !(left == right);
        }
    }
}