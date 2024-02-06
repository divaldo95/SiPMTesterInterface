using System;
using System.Text;

namespace SiPMTesterInterface.Classes
{
    public class MessageBufferIncosistencyEventArgs : EventArgs
    {
        public MessageBufferIncosistencyEventArgs() : base()
        {
        }
        public MessageBufferIncosistencyEventArgs(string msg) : base()
        {
            Message = msg;
        }
        public string Message { get; set; } = "";
    }

    public class MessageBuffer
	{
        private readonly int MaxMemoryCapacityMB = 10; // Adjust the limit as needed
        private long currentMemoryUsage = 0;

        private int CurrentMessageIndex = -1;
        public bool Inconsistent { get; private set; } = false;

        public List<string> Buffer { get; private set; } = new List<string>();

        public event EventHandler<MessageBufferIncosistencyEventArgs> OnMessageBufferIncosistency;

        public void Insert(string message)
        {
            // Calculate message size in bytes
            int messageSizeBytes = Encoding.UTF8.GetByteCount(message);

            // Check if adding the message exceeds the memory capacity
            if (currentMemoryUsage + messageSizeBytes > MaxMemoryCapacityMB * 1024 * 1024)
            {
                // If exceeds capacity, remove the oldest message (LIFO)
                if (Buffer.Count > 0)
                {
                    string removedMessage = Buffer[0];
                    int removedMessageSizeBytes = Encoding.UTF8.GetByteCount(removedMessage);

                    Buffer.RemoveAt(0);
                    currentMemoryUsage -= removedMessageSizeBytes;

                    Console.WriteLine($"Message removed from buffer: {removedMessage}");
                }
            }

            // Add the new message to the buffer
            Buffer.Add(message);
            currentMemoryUsage += messageSizeBytes;

            Console.WriteLine($"Message received and added to buffer: {message}");
        }

        /*
         * Keep track of incoming messages by its index.
         * If the arriving message index is not matches with saved index + 1
         * the buffer sets the Incosistent flag and fires an event so the necessary
         * actions can be taken.
         * Remember, if the Insert function is used, then this function can misbehave
         * on calculating the consistency.
         */
        public void InsertIndexed(string message, int index)
        {
            if (CurrentMessageIndex + 1 != index && !Inconsistent)
            {
                Inconsistent = true;
                OnMessageBufferIncosistency?.Invoke(this, new MessageBufferIncosistencyEventArgs());
            }
            Insert(message);
        }

        public void Clear()
        {
            Buffer.Clear();
            currentMemoryUsage = 0;
            Inconsistent = false;
        }

        public void ClearInconsistentFlag()
        {
            Inconsistent = false;
        }

        public MessageBuffer()
		{
		}

        public MessageBuffer(int capacity) //capacity in megabytes
        {
            MaxMemoryCapacityMB = capacity;
        }
    }
}

