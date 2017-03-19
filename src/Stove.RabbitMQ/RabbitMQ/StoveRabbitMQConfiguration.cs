﻿namespace Stove.RabbitMQ
{
    public class StoveRabbitMQConfiguration : IStoveRabbitMQConfiguration
    {
        public string HostAddress { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string QueueName { get; set; }

        public bool UseRetryMechanism { get; set; }

        public int MaxRetryCount { get; set; }
    }
}
