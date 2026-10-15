using System;
using Microsoft.Xna.Framework;

namespace CalamityRelics.HelperScripts
{
    public enum MirrorOriginType
    {
        HORIZONTAL,
        VERTICAL,
        BOTH,
        NONE
    }
    
    // Used to create certain world-local space relationships to assist doing logic or drawing sprites relative to a points position and non Axis-aligned rotation
    public struct Origin
    {
        public Vector2 position;
        public float rotation;
        public float rotationRad => rotation * MathF.PI / 180;

        public Origin(Vector2? position, float rotation = 0)
        {
            this.position = position?? Vector2.Zero;
            this.rotation = rotation;
        }

        public Origin(Vector2? position, Vector2 lookDir)
        {
            this.position = position?? Vector2.Zero;
            lookDir = Vector2.Normalize(lookDir);
            rotation = RotationFromVector(lookDir);
        }

        public static float RotationFromVector(Vector2 vector)
        {
            return ToDegrees(MathF.Atan2(vector.Y, vector.X));
        }

        public static float ToDegrees(float rad)
        {
            return rad * 180 / MathF.PI;
        }

        public Vector2 unitVector => new Vector2(
            MathF.Cos(rotationRad),
            MathF.Sin(rotationRad)
        );

        public Vector2 LocalToWorldSpace(Vector2 vector)
        {
            float rad = rotationRad;
            return position + new Vector2(
                vector.X * MathF.Cos(rad) - vector.Y * MathF.Sin(rad),
                vector.X * MathF.Sin(rad) + vector.Y * MathF.Cos(rad)
            );
        }

        public Vector2 WorldToLocalSpace(Vector2 vector)
        {
            float rad = -rotationRad;
            
            vector -= position;

            return new Vector2(
                vector.X * MathF.Cos(rad) + vector.Y * MathF.Sin(rad),
                -vector.X * MathF.Sin(rad) + vector.Y * MathF.Cos(rad)
            );
        }

        /// <summary>
        /// Translates the origin to support "flipping" relative to a given position.
        /// Created as a means to combat the annoying nature of having to account for flipping sprites in drawing, and needing important positions to be flipped correctly in both cases.
        /// </summary>
        /// <param name="mirrorPos">The position to mirror relative to</param>
        /// <param name="posMirrorType">Sets whether you want the point to be mirrored horizontally, vertically, both, or none. Defaults to Horizontal</param>
        /// <param name="rotMirrorType">Sets whether you want the origins rotation to be flipped horizontally, vertically, both, or none. Defaults to horizontal</param>
        /// <returns> Nothing, the origin you called this method on will be changed to reflect an image version of its past self, mirrored around your given parameters</returns>
        
        public void Mirror(Vector2 mirrorPos, MirrorOriginType posMirrorType = MirrorOriginType.HORIZONTAL, MirrorOriginType rotMirrorType = MirrorOriginType.HORIZONTAL)
        {
            Vector2 newUnitVector = unitVector;
            newUnitVector.X *= rotMirrorType == MirrorOriginType.HORIZONTAL    || rotMirrorType == MirrorOriginType.BOTH ? -1 : 1;
            newUnitVector.Y *= rotMirrorType == MirrorOriginType.VERTICAL      || rotMirrorType == MirrorOriginType.BOTH ? -1 : 1;

            rotation = ToDegrees(MathF.Atan2(newUnitVector.Y, newUnitVector.X));
            
            Vector2 difference = mirrorPos - position;
            difference.X *= posMirrorType == MirrorOriginType.HORIZONTAL    || posMirrorType == MirrorOriginType.BOTH       ? 1 : 0;
            difference.Y *= posMirrorType == MirrorOriginType.VERTICAL      || posMirrorType == MirrorOriginType.VERTICAL   ? 1 : 0;
            
            position -= difference * 2;

        }
    }

        // If I want the origin to update to a live position and rotation value from another variable. 
        // (Unused for now, I want to experiment with this but may be useless if manually changing the position is just viable on its own)
        public ref struct RefOrigin
        {
            public ref Vector2 position;
            public ref float rotation;

            public float rotationRad => rotation * MathF.PI / 180;
            public Origin parent;

            public RefOrigin(ref Vector2 position, ref float rotation)
            {
                this.position = ref position;
                this.rotation = ref rotation;
            }

            // public RefOrigin(ref Vector2 position, Vector2 lookDir)
            // {
            //     this.position = ref position;
            //     lookDir = Vector2.Normalize(lookDir);
            //     rotation = ref ToDegrees(MathF.Atan2(lookDir.Y, lookDir.X));
            // }

            public static float ToDegrees(float rad)
            {
                return rad * 180 / MathF.PI;
            }

            public Vector2 unitVector => new Vector2(
                MathF.Cos(rotationRad),
                MathF.Sin(rotationRad)
            );

            public Vector2 LocalToWorldSpace(Vector2 vector)
            {
                float rad = rotationRad;
                return position + new Vector2(
                 vector.X * MathF.Cos(rad) - vector.Y * MathF.Sin(rad),
                 vector.X * MathF.Sin(rad) + vector.Y * MathF.Cos(rad)
                );
            }

            public Vector2 WorldToLocalSpace(Vector2 vector)
            {
                float rad = -rotationRad;
                
                vector -= position;

                return new Vector2(
                 vector.X * MathF.Cos(rad) + vector.Y * MathF.Sin(rad),
                 -vector.X * MathF.Sin(rad) + vector.Y * MathF.Cos(rad)
                );
            }
        }
}