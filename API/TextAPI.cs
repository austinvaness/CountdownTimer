using ProtoBuf;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game.ModAPI;
using VRageMath;

namespace avaness.ServerTextAPI.API
{
    public class TextAPI
    {
        public const long MessageId = 2352436226;
        public const ushort PacketId = (ushort)(MessageId % ushort.MaxValue);

        public enum TextAlignment
        {
            Left = -1,
            Center = 0,
            Right = 1
        }

        [ProtoContract]
        public class Text
        {
            [ProtoMember(1)]
            public string id;

            [ProtoMember(2)]
            public long lengthTicks;
            public TimeSpan Length
            {
                get
                {
                    return new TimeSpan(lengthTicks);
                }
                set
                {
                    lengthTicks = value.Ticks;
                }
            }

            [ProtoMember(3)]
            public string text;

            [ProtoMember(4)]
            public float centerX;
            [ProtoMember(5)]
            public float centerY;
            public Vector2D Center
            {
                get
                {
                    return new Vector2D(centerX, centerY);
                }
                set
                {
                    centerX = (float)value.X;
                    centerY = (float)value.Y;
                }
            }

            [ProtoMember(6)]
            public float scale;

            [ProtoMember(7)]
            public int align;
            public TextAlignment Alignment
            {
                get
                {
                    return (TextAlignment)align;
                }
                set
                {
                    align = (int)value;
                }
            }

            [ProtoMember(8)]
            public string font;

            private readonly List<IMyPlayer> temp = new List<IMyPlayer>();

            /// <summary>
            /// Used for serialization only.
            /// </summary>
            public Text()
            {

            }

            /// <summary>
            /// Creates a text object.
            /// </summary>
            /// <param name="id">The text object id.</param>
            /// <param name="length">The amount of time the text will be on the screen.</param>
            /// <param name="text">The text to display.</param>
            /// <param name="center">The position of the text in Text Hud API coordinates. Top Right = (1,1) Bottom Left = (-1,-1)</param>
            /// <param name="scale">The scale of the text.</param>
            /// <param name="alignment">The horizontal alignment of the text.</param>
            /// <param name="font">The font name of the text. By default, only white and monospace are allowed.</param>
            public Text(string id, TimeSpan length, string text, Vector2D center, float scale, TextAlignment alignment = TextAlignment.Left, string font = "white")
            {
                this.id = id;
                Length = length;
                this.text = text;
                Center = center;
                this.scale = scale;
                Alignment = alignment;
                this.font = font;
            }

            public void Delete()
            {
                text = null;
            }

            private byte[] Serialize()
            {
                return MyAPIGateway.Utilities.SerializeToBinary<Text>(this);
            }

            /// <summary>
            /// Updates the text on all matching player's hud.
            /// </summary>
            /// <param name="filter">Function that returns true if the player should be sent the hud.</param>
            public void SendToAll(Func<IMyPlayer, bool> filter = null)
            {
                ulong? myId = MyAPIGateway.Session.Player?.SteamUserId;
                byte[] data = Serialize();
                if(filter == null)
                {
                    MyAPIGateway.Multiplayer.SendMessageToOthers(PacketId, data);
                    if (myId.HasValue)
                        MyAPIGateway.Utilities.SendModMessage(MessageId, data);
                }
                else
                {
                    MyAPIGateway.Players.GetPlayers(temp, filter);
                    foreach (IMyPlayer p in temp)
                    {
                        if (myId.HasValue && p.SteamUserId == myId.Value)
                            MyAPIGateway.Utilities.SendModMessage(MessageId, data);
                        else
                            MyAPIGateway.Multiplayer.SendMessageTo(PacketId, data, p.SteamUserId);
                    }
                    temp.Clear();
                }
            }

            /// <summary>
            /// Updates the text on a specific player's hud.
            /// </summary>
            /// <param name="id">The steam id of the player.</param>
            public void SendTo(ulong id)
            {
                byte[] data = Serialize();
                IMyPlayer me = MyAPIGateway.Session.Player;
                if (me != null && me.SteamUserId == id)
                    MyAPIGateway.Utilities.SendModMessage(MessageId, data);
                else
                    MyAPIGateway.Multiplayer.SendMessageTo(PacketId, data, id);
            }
        }
    }
}
