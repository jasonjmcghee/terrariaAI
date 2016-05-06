using Microsoft.Xna.Framework;
using System;

namespace AIRefactor {
    class Physics {

        public static float QuadraticFormula(float a, float b, float c) {
            float result = float.NaN;

            if (a == 0 && b != 0) {
                result = -c / b;
            } else {
                float partial = (float)Math.Pow(b, 2) - 4.0f * a * c;
                if (partial >= 0) {
                    result = (-b + (float)Math.Sqrt(partial)) / (2.0f * a);
                }
            }

            return result >= 0 ? result : float.NaN;
        }

        public static float SolveForTime(float velocity, float deltaVelocity, float x) {
            float time = QuadraticFormula(deltaVelocity / 2.0f, velocity, -x);
            return time > 0 ? time : float.MaxValue;
        }

        public static Vector2 InstantaneousVelocity(Vector2 velocity, Vector2 acceleration, float time) {
            float velocityX = velocity.X + acceleration.X * time;
            float velocityY = velocity.Y + acceleration.Y * time;
            return new Vector2(velocityX, velocityY);
        }

        public static Vector2 InstantaneousPosition(Vector2 position, Vector2 velocity, Vector2 acceleration, float time) {
            float deltaPositionX = LocationXAtTime(position, velocity, acceleration, time);
            float deltaPositionY = LocationYAtTime(position, velocity, acceleration, time);
            return new Vector2(deltaPositionX, deltaPositionY);
        }

        public static float LocationXAtTime(Vector2 position, Vector2 velocity, Vector2 acceleration, float time) {
            return position.X + velocity.X * time + 0.5f * acceleration.X * (float)Math.Pow(time, 2);
        }

        public static float LocationYAtTime(Vector2 position, Vector2 velocity, Vector2 acceleration, float time) {
            return position.Y + velocity.Y * time + 0.5f * acceleration.Y * (float)Math.Pow(time, 2);
        }

        public static float LocationXGivenY(Vector2 position, Vector2 velocity, Vector2 acceleration, float y) {
            float time = SolveForTime(velocity.Y, acceleration.Y, y);
            return LocationXAtTime(position, velocity, acceleration, time);
        }

        public static float LocationYGivenX(Vector2 position, Vector2 velocity, Vector2 acceleration, float x) {
            float time = SolveForTime(position.X, velocity.X, x);
            return LocationYAtTime(position, velocity, acceleration, time);
        }

        public static bool Between (float x, float s1, float s2) {
            return x >= s1 && x <= s2;
        }

        public static bool Intersects (Vector2 centerA, float widthA, float heightA,
            Vector2 centerB, float widthB, float heightB) {
            bool leftIntersects = Between(centerA.X - widthA / 2, centerB.X - widthB / 2, centerB.X + widthB / 2);
            bool rightIntersects = Between(centerA.X + widthA / 2, centerB.X - widthB / 2, centerB.X + widthB / 2);
            bool topIntersects = Between(centerA.Y - heightA / 2, centerB.Y - heightB / 2, centerB.Y + heightB / 2);
            bool bottomIntersects = Between(centerA.Y + heightA / 2, centerB.Y - heightB / 2, centerB.Y + heightB / 2);

            return (leftIntersects || rightIntersects) && (topIntersects || bottomIntersects);
        }

        public static bool WillCollide(Vector2 positionA, float widthA, float heightA,
                Vector2 positionB, float widthB, float heightB) {

            bool leftBeforeLeft = positionA.X - widthA / 2 <= positionB.X - widthB / 2;
            bool rightPastRight = positionA.X + widthA / 2 >= positionB.X + widthB / 2;
            bool rightPastLeft = positionA.X + widthA / 2 >= positionB.X - widthB / 2;
            bool leftBeforeRight = positionA.X - widthA / 2 <= positionB.X + widthB / 2;

            bool bottomPastBottom = positionA.Y + heightA / 2 >= positionB.Y + heightB / 2;
            bool topBeforeTop = positionA.Y - heightA / 2 <= positionB.Y - heightB / 2;
            bool bottomPastTop = positionA.Y + heightA / 2 >= positionB.Y - heightB / 2;
            bool topBeforeBottom = positionA.Y - heightA / 2 <= positionB.Y + heightB / 2;

            bool xCollision = (rightPastLeft && leftBeforeLeft) || (leftBeforeRight && rightPastRight);
            bool yCollision = (bottomPastTop && topBeforeTop) || (topBeforeBottom && bottomPastBottom);

            return xCollision && yCollision;
        }

        public static Tuple<float, side> Collision (Vector2 positionA, Vector2 velocityA, Vector2 accelerationA, float widthA, float heightA,
            Vector2 positionB, Vector2 velocityB, Vector2 accelerationB, float widthB, float heightB) {

            float hitTimeY1, hitTimeY2, hitTimeX1, hitTimeX2;

            // Bottom of A Hitting Top of B
            float hitLocation = (positionB.Y - heightB / 2) - (positionA.Y + heightA / 2);
            hitTimeY1 = Physics.SolveForTime(velocityA.Y - velocityB.Y, accelerationA.Y - accelerationB.Y, hitLocation);
            // Top of A Hitting Bottom of B
            hitLocation = (positionA.Y - heightA / 2) - (positionB.Y + heightB / 2);
            hitTimeY2 = Physics.SolveForTime(velocityA.Y - velocityB.Y, accelerationA.Y - accelerationB.Y, hitLocation);
            // Left of A Hitting Right of B
            hitLocation = (positionB.X + widthB / 2) - (positionA.X - widthA / 2);
            hitTimeX1 = Physics.SolveForTime(velocityA.X - velocityB.X, accelerationA.X - accelerationB.X, hitLocation);
            // Right of A Hitting Left of B
            hitLocation = (positionA.X + widthA / 2) - (positionB.X - widthB / 2);
            hitTimeX2 = Physics.SolveForTime(velocityA.X - velocityB.X, accelerationA.X - accelerationB.X, hitLocation);

            float time = Math.Min(Math.Min(hitTimeY1, hitTimeY2), Math.Min(hitTimeX1, hitTimeX2));
            side hitSide;
            if (time == float.MaxValue) {
                hitSide = side.NONE;
            } else if (time == hitTimeX1) {
                hitSide = side.LEFT;
            } else if (time == hitTimeY1) {
                hitSide = side.TOP;
            } else if (time == hitTimeX2) {
                hitSide = side.RIGHT;
            } else {
                hitSide = side.BOTTOM;
            }
            return Tuple.Create<float, side>(time, hitSide);
        }

        public enum side {
            LEFT,
            TOP,
            RIGHT,
            BOTTOM,
            NONE
        }
    }
}
