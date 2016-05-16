using Microsoft.Xna.Framework;
using System;
using Terraria;

namespace AIRefactor.Wrappers {
    class ProjectileWrapper {

        public Projectile projectile;
        public int height;
        public int width;

        public ProjectileWrapper (Projectile projectile) {
            this.projectile = projectile;
            height = projectile.height;
            width = projectile.width;
        }

        // Return the earliest time a projectile will hit
        public Tuple<float, Physics.side> Hit(NPC npc) {
            Vector2 npcAcceleration = npc.oldVelocity.Equals(Vector2.Zero) ? Vector2.Zero : npc.velocity - npc.oldVelocity;
            return Hit(npc.position, npc.velocity, Vector2.Zero, npc.width, npc.height);
        }

        public Tuple<float, Physics.side> Hit(Vector2 position, Vector2 velocity, Vector2 acceleration,
            float w, float h) {
            return Physics.Collision(projectile.position, projectile.velocity, projectile.velocity - projectile.oldVelocity, width, height,
                position, velocity, acceleration, w, h);
        }

        public Vector2 InstantaneousVelocity(float time) {
            return Physics.InstantaneousVelocity(projectile.velocity, getAcceleration(), time);
        }

        public Vector2 InstantaneousPosition(float time) {
            return Physics.InstantaneousPosition(projectile.position, projectile.velocity, getAcceleration(), time);
        }

        public float LocationXAtTime(float time) {
            return Physics.LocationXAtTime(projectile.position, projectile.velocity, getAcceleration(), time);
        }

        public float LocationYAtTime(float time) {
            return Physics.LocationYAtTime(projectile.position, projectile.velocity, getAcceleration(), time);
        }

        public float LocationXGivenY(float y) {
            return Physics.LocationXGivenY(projectile.position, projectile.velocity, getAcceleration(), y);
        }

        public float LocationYGivenX(float x) {
            return Physics.LocationYGivenX(projectile.position, projectile.velocity, getAcceleration(), x);
        }

        public bool WillCollideWithNPC (NPC npc, float time) {
            Vector2 npcAcceleration = npc.oldVelocity.Equals(Vector2.Zero) ? Vector2.Zero : npc.velocity - npc.oldVelocity;
            Vector2 npcFuturePosition = Physics.InstantaneousPosition(npc.position, npc.velocity, Vector2.Zero, time);
            return WillCollide(npcFuturePosition, npc.width, npc.height, time);
        }

        public bool WillCollideWithNPC(NPC npc, Vector2 acceleration, float time) {
            Vector2 npcFuturePosition = Physics.InstantaneousPosition(npc.position, npc.velocity, acceleration, time);
            return WillCollide(npcFuturePosition, npc.width, npc.height, time);
        }

        public bool WillCollide(Vector2 targetPosition, int targetWidth, int targetHeight, float time) {
            return Physics.Intersects(InstantaneousPosition(time + 0.1f), width, height, 
                targetPosition, targetWidth, targetHeight) || Physics.Intersects(InstantaneousPosition(time - 0.1f), width, height,
                targetPosition, targetWidth, targetHeight);
        }

        public Vector2 getAcceleration() {
            if (projectile.oldVelocity.Equals(Vector2.Zero)) {
                return Vector2.Zero;
            } else {
                return projectile.velocity - projectile.oldVelocity;
            }
        }
    }
}
