﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReplayInitData.cs" company="SC2ReplayParser">
//   Copyright © 2012 All Rights Reserved
// </copyright>
// <summary>
//   Parses the replay.Initdata file in the replay file.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Starcraft2.ReplayParser
{
    using System.IO;
    using System.Text;

    using Starcraft2.ReplayParser.Streams;

    /// <summary> Parses the replay.Initdata file in the replay file. </summary>
    public class ReplayInitData
    {
        #region Public Methods

        /// <summary> Parses the replay.initdata file in a replay file. </summary>
        /// <param name="replay"> The replay file to apply the parsed data to. </param>
        /// <param name="buffer"> The buffer containing the replay.initdata file. </param>
        public static void Parse(Replay replay, byte[] buffer)
        {
            
            using (var stream = new MemoryStream(buffer))
            {
                var reader = new BitReader(stream);
                
                var i = reader.ReadByte();

                var playerList = new string[i];
                for (int j = 0; j < i; j++)
                {
                    var nameLength = reader.ReadByte();
                    reader.AlignToByte();
                    var str = reader.ReadString(nameLength);

                    playerList[j] = str;

                    if (replay.ReplayBuild < 24764)
                    {
                        reader.ReadBytes(5);
                    }
                    else
                    {
                        if (reader.ReadBoolean())
                        {
                            var strLength = reader.ReadByte();
                            reader.AlignToByte();
                            var clanTag = reader.ReadString(strLength);                                
                        }

                        if (reader.ReadBoolean())
                        {
                            reader.ReadByte(); // Highest league
                        }

                        if (reader.ReadBoolean())
                        {
                            var swarmLevel = reader.ReadInt32(); // Swarm level
                        }

                        reader.ReadInt32(); // Random seed

                        if (reader.ReadBoolean())
                        {
                            reader.ReadByte(); // Race Preference
                        }

                        if (reader.ReadBoolean())
                        {
                            reader.ReadByte(); // Team Preference
                        }

                        reader.ReadBoolean(); //test map
                        reader.ReadBoolean(); //test auto
                        reader.ReadBoolean(); //examine
                        reader.ReadBoolean(); //custom interface
                        reader.Read(2);   //observer
                    }
                }

                if (replay.ReplayBuild < 24764)
                {
                    using (var binaryReader = new BinaryReader(stream))
                    {
                        // Save the full list of clients.
                        // This is no longer necessary since we get the client list elsewhere.
                        //// replay.ClientList = playerList;   
                        if (PositionAfter(binaryReader, new byte[] { 115, 50, 109, 97 }))
                        {
                            reader.ReadBytes(2);
                            var gatewayStr = reader.ReadBytes(2);

                            var gateway = Encoding.UTF8.GetString(gatewayStr);
                            replay.Gateway = gateway;
                        }
                        else
                        {
                            replay.GameType = GameType.SinglePlayer;
                        }
                    }
                }
            }
        }

        #endregion

        #region Methods

        /// <summary> Advances the position of the reader to the bytes following the expected array of bytes. </summary>
        /// <param name="reader"> The reader, which will be advanced. </param>
        /// <param name="paramArrayOfByte"> The array of bytes expected to be found in the reader. </param>
        /// <returns> Returns a value indicating whether the array of bytes were found (and whether the reader was advanced). </returns>
        protected static bool PositionAfter(BinaryReader reader, byte[] paramArrayOfByte)
        {
            var i = reader.BaseStream.Position;

            var stream = reader.BaseStream;

            var k = stream.Length - paramArrayOfByte.Length;

            var arr = reader.ReadBytes((int)k);

            int j;
            for (j = 0; j < k; j++)
            {
                int l;
                for (l = 0; l < paramArrayOfByte.Length; l++)
                {
                    if (arr[j + l] != paramArrayOfByte[l])
                    {
                        break;
                    }
                }

                if (l == paramArrayOfByte.Length)
                {
                    break;
                }
            }

            if (j < k)
            {
                reader.BaseStream.Position = i + j + paramArrayOfByte.Length;
                return true;
            }

            reader.BaseStream.Position = i;
            return false;
        }

        /// <summary> Reads a UTF-8 string from the current position of the BinaryReader. </summary>
        /// <param name="reader"> The reader. </param>
        /// <returns> The read string. </returns>
        private static string ReadString(BinaryReader reader)
        {
            var numBytes = reader.ReadByte();
            var strArr = reader.ReadBytes(numBytes);

            return Encoding.UTF8.GetString(strArr);
        }

        #endregion
    }
}