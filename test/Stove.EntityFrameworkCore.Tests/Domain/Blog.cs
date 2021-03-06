﻿using System;
using System.Collections.Generic;

using Stove.Domain.Entities;
using Stove.Domain.Entities.Auditing;
using Stove.EntityFrameworkCore.Tests.Domain.Events;

namespace Stove.EntityFrameworkCore.Tests.Domain
{
    public class Blog : AggregateRoot, IHasCreationTime, ISoftDelete
    {
        public Blog()
        {
            Register<BlogUrlChangedEvent>(When);
        }

        public Blog(string name, string url) : this()
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            Name = name;
            Url = url;

            ApplyChange(
                new BlogCreatedEvent(name)
                );
        }

        public virtual string Name { get; set; }

        public virtual string Url { get; protected set; }

        public virtual ICollection<Post> Posts { get; set; }

        public virtual DateTime CreationTime { get; set; }
        
        public bool IsDeleted { get; set; }

        private void When(BlogUrlChangedEvent @event)
        {
            Url = @event.Url;
        }

        public virtual void ChangeUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            ApplyChange(new BlogUrlChangedEvent(this, url));
        }
    }
}
