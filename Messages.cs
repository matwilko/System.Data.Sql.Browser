﻿using System.IO;
using System.Text;

namespace System.Data.Sql.Browser
{
    internal static class Messages
    {
        /// <summary>
        /// The CLNT_BCAST_EX packet is a broadcast or multicast request that is generated by clients that are trying to identify the list of database instances on the network and their network protocol connection information.
        /// </summary>
        /// <seealso href="http://msdn.microsoft.com/en-us/library/cc219743.aspx">2.2.1 CLNT_BCAST_EX</seealso>
        /// <returns>Array of bytes containing the CLNT_BCAST_EX message</returns>
        public static byte[] ClientBroadcastEx()
        {
            return new byte[] { 0x02 };
        }

        /// <summary>
        /// The CLNT_UCAST_EX packet is a unicast request that is generated by clients that are trying to identify the list of database instances and their network protocol connection information installed on a single machine. The client generates a UDP packet with a single byte.
        /// </summary>
        /// <seealso href="http://msdn.microsoft.com/en-us/library/cc219745.aspx">2.2.2 CLNT_UCAST_EX</seealso>
        /// <returns>Array of bytes containing the CLNT_UCAST_EX message</returns>
        public static byte[] ClientUnicastEx()
        {
            return new byte[] { 0x03 };
        }

        /// <summary>
        /// The CLNT_UCAST_INST packet is a request for information related to a specific instance.
        /// </summary>
        /// <param name="instanceName">Name of the instance to request information about.</param>
        /// <returns>Array of bytes containing the CLNT_UCAST_INST message</returns>
        public static byte[] ClientUnicastInstance(string instanceName)
        {
            var instanceNameBytes = GetInstanceNameBytes(instanceName);

            var returnBytes = new byte[2 + instanceNameBytes.Length];
            returnBytes[0] = 0x04;
            returnBytes[returnBytes.Length - 1] = 0x00;

            Array.Copy(instanceNameBytes, 0, returnBytes, 1, instanceNameBytes.Length);

            return returnBytes;
        }

        /// <summary>
        /// The CLNT_UCAST_DAC packet request is used to determine the TCP port on which the SQL Server dedicated administrator connection (DAC) endpoint is listening.
        /// </summary>
        /// <param name="instanceName">Name of the instance to request information about.</param>
        /// <returns>Array of bytes containing the CLNT_UCAST_DAC message</returns>
        public static byte[] ClientUnicastDac(string instanceName)
        {
            var instanceNameBytes = GetInstanceNameBytes(instanceName);

            var returnBytes = new byte[3 + instanceNameBytes.Length];
            returnBytes[0] = 0x0F;
            returnBytes[1] = 0x01;
            returnBytes[returnBytes.Length - 1] = 0x00;

            Array.Copy(instanceNameBytes, 0, returnBytes, 2, instanceNameBytes.Length);

            return returnBytes;
        }

        /// <summary>
        /// The server responds to all client requests with an SVR_RESP.
        /// </summary>
        /// <param name="response">The bytes that were received in response.</param>
        /// <returns>The string returned from the server.</returns>
        public static string ServerResponse(byte[] response)
        {
            if (response[0] != 0x05)
            {
                throw new InvalidDataException("Invalid SVR_RESP message");
            }

            int size = (response[2] << 8) + response[1];

            return Encoding.Default.GetString(response, 3, size);
        }

        /// <summary>
        /// The format of the SVR_RESP is different for a CLNT_UCAST_DAC request only.
        /// </summary>
        /// <param name="response">The bytes that were received in response.</param>
        /// <returns>The value of the TCP port number that is used for DAC.</returns>
        public static int ServerResponseDac(byte[] response)
        {
            if (response.Length != 6
                || response[0] != 0x05 // SVR_RESP
                || response[1] != 0x06 || response[2] != 0x00 // RESP_SIZE 0x06 over 2 bytes (little-endian)
                || response[3] != 0x01 // PROTOCOL_VERSION
                )
            {
                throw new InvalidDataException("Invalid SVR_RESP (DAC) message");
            }

            return (response[4] << 8) + response[3];
        }

        private static byte[] GetInstanceNameBytes(string instanceName)
        {
            if (string.IsNullOrWhiteSpace(instanceName))
            {
                throw new ArgumentException("Instance name cannot be empty", "instanceName");
            }

            if (instanceName.Length > 32)
            {
                throw new ArgumentOutOfRangeException("instanceName", "Instance name cannot be longer than 32 characters");
            }

            var byteCount = Encoding.Default.GetByteCount(instanceName);

            if (byteCount > 32)
            {
                throw new ArgumentOutOfRangeException("instanceName",
                    "Instance name encodes to greater than 32 bytes");
            }

            return Encoding.Default.GetBytes(instanceName);
        }
    }
}