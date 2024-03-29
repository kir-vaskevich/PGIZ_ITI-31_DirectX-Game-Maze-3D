﻿using SharpDX;
using DirectLib.Engine.Objects;

namespace DirectLib.Engine.Colliders
{
    public abstract class Collider
    {
        public Vector3 Position { get; protected internal set; }

        public bool Collide { get; set; }

        public Collider(Vector3 pos)
        {
            Position = pos;
        }

        public virtual bool IsCollided(GameObject gameObject)
        {
            return false;
        }
    }
}
