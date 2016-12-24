﻿using NetMQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageWire
{
    public class Message
    {
        public Guid ClientId { get; set; }
        public List<byte[]> Frames { get; set; }
    }

    internal static class MessageExtensions
    {
        public static Message ToMessageWithoutClientFrame(this NetMQMessage msg, Guid clientId)
        {
            if (msg == null || msg.FrameCount == 0) return null;
            List<byte[]> frames = new List<byte[]>();
            if (msg.FrameCount > 0)
            {
                frames = (from n in msg where !n.IsEmpty select n.Buffer).ToList();
            }
            return new Message
            {
                ClientId = clientId,
                Frames = frames
            };
        }

        public static Message ToMessageWithClientFrame(this NetMQMessage msg)
        {
            if (msg == null || msg.FrameCount == 0) return null;
            if (msg[0].BufferSize != 16) return null; //must have a Guid id
            var clientId = new Guid(msg[0].Buffer);
            List<byte[]> frames = new List<byte[]>();
            if (msg.FrameCount > 1)
            {
                frames = (from n in msg where !n.IsEmpty select n.Buffer).Skip(1).ToList();
            }
            return new Message
            {
                ClientId = clientId,
                Frames = frames
            };
        }

        public static NetMQMessage ToNetMQMessage(this Message msg)
        {
            var message = new NetMQMessage();
            message.Append(msg.ClientId.ToByteArray());
            message.AppendEmptyFrame();
            if (null != msg.Frames)
            {
                foreach (var frame in msg.Frames)
                {
                    message.Append(frame);
                }
            }
            else
            {
                message.AppendEmptyFrame();
            }
            return message;
        }
    }

    public class MessageEventArgs : EventArgs
    {
        public Message Message { get; set; }
    }

    public class MessageEventFailureArgs : EventArgs
    {
        public MessageFailure Failure { get; set; }
    }

    public class MessageFailure
    {
        public string ErrorMessage { get; set; }
        public string ErrorCode { get; set; }
        public Message Message { get; set; }
    }
}
