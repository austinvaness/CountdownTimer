using ProtoBuf;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage;
using VRage.Game.ModAPI;
using VRageMath;

namespace avaness.CountdownTimer.API
{
    public class TimerAPI
    {
        public const long MessageId = 2352436226;
        public const ushort PacketId = (ushort)(MessageId % ushort.MaxValue);

        [ProtoContract]
        public class Timer
        {
            [ProtoMember(1)]
            public string id;

            [ProtoMember(2)]
            public long lengthTicks;
            public TimeSpan length
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
            public string timerFormat;

            [ProtoMember(5)]
            public double centerX;
            [ProtoMember(6)]
            public double centerY;
            public Vector2D center 
            {
                get
                {
                    return new Vector2D(centerX, centerY);
                }
                set
                {
                    centerX = value.X;
                    centerY = value.Y;
                }
            }

            [ProtoMember(7)]
            public double scale;

            [ProtoMember(8)]
            public bool down;

            private readonly List<IMyPlayer> temp = new List<IMyPlayer>();

            /// <summary>
            /// Used for serialization only.
            /// </summary>
            public Timer()
            {

            }

            /// <summary>
            /// Creates a timer object.
            /// </summary>
            /// <param name="id">The timer id.</param>
            /// <param name="length">The amount of time the timer will be running.</param>
            /// <param name="text">The text of the timer. Must include {0} to represent the time.</param>
            /// <param name="timerFormat">The formatting of the time. See TimeSpan format strings.</param>
            /// <param name="center">The center of the timer in Text Hud API coordinates. Top Right = (1,1) Bottom Left = (-1,-1)</param>
            /// <param name="scale">The scale of the text.</param>
            /// <param name="down">If <see langword="true"/>, the timer will count down instead of up.</param>
            public Timer(string id, TimeSpan length, string text, string timerFormat, Vector2D center, double scale, bool down = true)
            {
                this.id = id;
                this.lengthTicks = length.Ticks;
                this.text = text;
                this.timerFormat = timerFormat;
                this.centerX = center.X;
                this.centerY = center.Y;
                this.scale = scale;
                this.down = down;
            }

            private byte[] Serialize()
            {
                return MyAPIGateway.Utilities.SerializeToBinary<Timer>(this);
            }

            /// <summary>
            /// Displays the timer on all matching player's hud.
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
            /// Displays the timer on a specific player's hud.
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
