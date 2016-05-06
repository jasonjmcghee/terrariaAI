using AIRefactor.Wrappers;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AIRefactor.NPCs {
    class RangedNPC : ModNPC {

        private int jump;
        private float jumpSpeed = Player.jumpSpeed;
        private int jumpHeight = Player.jumpHeight;
        private Vector2 lastPosition = new Vector2(-1, -1);
        private bool atLastPosition = true;
        private ProjectileWrapper threat = null;
        private delegate float ThreatFunction(Projectile projectile);
        private bool inDanger = false;
        private bool wasInDanger = false;
        private Tuple<float, Physics.side> hit;
        float timeUntilProjectileCollides;

        public override void SetDefaults() {
            npc.CloneDefaults(NPCID.GoblinArcher);
            Main.npcFrameCount[npc.type] = 25;
            NPCID.Sets.ExtraFramesCount[npc.type] = 9;
            NPCID.Sets.AttackFrameCount[npc.type] = 4;
            NPCID.Sets.AttackType[npc.type] = 0;
            NPCID.Sets.AttackTime[npc.type] = 90;
            NPCID.Sets.AttackAverageChance[npc.type] = 30;
            npc.aiStyle = -1;
            npc.stepSpeed = 2;
            npc.damage = 0;
            npc.immortal = true;
            animationType = NPCID.Guide;
        }

        /*public override bool PreAI() {
            npc.TargetClosest();
            return true;
        }*/

        private bool AttemptToMoveToPositionX (float positionX) {
            // If a real number
            if (positionX != positionX || npc.velocity.Y != 0) {
                return false;
            }
            float dist = npc.position.X - positionX;
            // If this should be our last step
            if (Math.Abs(dist) <= npc.stepSpeed) {
                float currentVelocityX = npc.velocity.X;
                npc.velocity.X = 0;
                if (threat != null && WillHitNPC(threat, threat.Hit(npc).Item1)) {
                    npc.velocity.X = currentVelocityX;
                } else {
                    return true;
                }
            }

            // If npc got knocked back and hit the ground, and still has x velocity away from target that hit npc
            if (Math.Sign(dist) + Math.Sign(npc.velocity.X) != 0) {
                if (Math.Abs(npc.velocity.X) > npc.stepSpeed) {
                    npc.velocity.X /= 5;
                } else {
                    float currentVelocityX = npc.velocity.X;
                    npc.velocity.X = -npc.stepSpeed * Math.Sign(dist);
                    if (threat != null && !inDanger && WillHitNPC(threat, threat.Hit(npc).Item1)) {
                        npc.velocity.X = 0;
                    } else {
                        // For target player code
                        return true;
                    }
                }
            } else {
                float currentVelocityX = npc.velocity.X;
                npc.velocity.X = -npc.stepSpeed * Math.Sign(dist);
                if (threat != null && WillHitNPC(threat, threat.Hit(npc).Item1)) {
                    npc.velocity.X = currentVelocityX;
                }
            }
            return false;
        }

        private float InverseProjectileDistance (Projectile projectile) {
            return projectile.damage / projectile.Distance(npc.position);
        }

        private ProjectileWrapper LargestThreat(ThreatFunction threatFunction) {
            Projectile biggestThreat = null;
            float biggestThreatScore = 0;

            foreach (Projectile projectile in Main.projectile) {
                if (!projectile.npcProj && projectile.type != 0 && projectile.active) {
                    float threatFunctionResult = threatFunction(projectile);
                    if (threatFunctionResult > biggestThreatScore) {
                        biggestThreat = projectile;
                        biggestThreatScore = threatFunctionResult;
                        return new ProjectileWrapper(biggestThreat);
                    }
                }
            }
            return null;
        }

        private bool WillHitNPC (ProjectileWrapper projectile, float frames) {
            if (projectile != null) {
                return projectile.WillCollideWithNPC(npc, frames);
            }
            return false;
        }

        private void Jump () {
            npc.velocity.Y = -jumpSpeed;
            jump = jumpHeight / 2;
        }

        private bool WillHitNPCOnBottomHalf (ProjectileWrapper projectile, float frames) {
            if (projectile != null) {
                return projectile.WillCollide(npc.position, npc.width, npc.height / 2, frames);
            }
            return false;
        }

        /**
         * If the projectile will hit our bottom half, move away from the projectile
         * If the projectile will hit our top half, move towards the projectile
         */
        private float NearestSafeX (ProjectileWrapper wrapper, float frames) {
            int direction = Math.Sign(wrapper.projectile.velocity.X) >= 0 ? 1 : -1;
            float offset = (npc.width / 2 + wrapper.width / 2 + 1) * direction;
            float hitY = npc.position.Y;
            if (WillHitNPCOnBottomHalf(wrapper, frames)) {
                hitY += npc.height / 2;
            } else {
                hitY -= npc.height / 2;
            }
            return wrapper.LocationXGivenY(hitY) + offset;
        }

        public override void AI() {

            if (npc.velocity.Y == 0) {
                // In the future we should try to make each move dodge as much damage / knockback? as possible
                threat = LargestThreat(InverseProjectileDistance);
                if (lastPosition.Equals(new Vector2(-1, -1))) {
                    lastPosition = npc.position;
                }
                if (threat != null) {
                    npc.ai[1]++;
                }
            }
            if (npc.ai[1] > 1) {
                if (threat != null) {

                    hit = threat.Hit(npc);
                    float hitTime = hit.Item1;
                    Physics.side hitSide = hit.Item2;

                    if (hitTime != -1) {
                        Vector2 velocityOnHit = threat.InstantaneousVelocity(hitTime);

                        inDanger = WillHitNPC(threat, hitTime);
                        if (inDanger) {
                            if (hitSide == Physics.side.LEFT || hitSide == Physics.side.RIGHT) {
                                Jump();
                                threat = null;
                                npc.ai[1] = 0;
                                inDanger = false;
                                wasInDanger = true;
                            } else if (hitSide == Physics.side.TOP || hitSide == Physics.side.BOTTOM) {
                                if (AttemptToMoveToPositionX(NearestSafeX(threat, hitTime))) {
                                    npc.ai[1] = 0;
                                    npc.ai[0] = -hitTime;
                                    threat = null;
                                    inDanger = false;
                                }
                            }
                        }
                    }
                }
            }

            if (npc.justHit) {
                threat = null;
                inDanger = false;
            }

            if (!inDanger && npc.ai[0] > 1 && Math.Abs(Main.player[Main.myPlayer].position.X - npc.position.X) > 1) {
                AttemptToMoveToPositionX(Main.player[Main.myPlayer].position.X);
            }

            if (jump > 0) {
                if (npc.velocity.Y == 0.0) {
                    jump = 0;
                } else {
                    npc.velocity.Y = -Player.jumpSpeed;
                    jump -= 1;
                }
            }
            if (npc.velocity.Y == 0) {
                npc.ai[0]++;
            }
            if (npc.ai[1] == 1) {
                npc.ai[1]++;
            }
        }

    }
}
