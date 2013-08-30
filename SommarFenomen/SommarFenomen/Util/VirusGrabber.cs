using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using FarseerPhysics.Collision;
using SommarFenomen.Objects;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using System.Diagnostics;

namespace SommarFenomen.Util
{
    class VirusGrabber
    {
        private Vector2 _handCenter;
        private Virus _grabbedVirus;
        public enum Hand { LEFT, RIGHT }
        private Hand _hand;
        private Sprite _handSprite;
        private World _world;
        
        private static readonly double OUTER_BODY_CLOSED_TIME = 0.1;
        private Stopwatch _outerBodyWatch = new Stopwatch();


        public VirusGrabber(Hand hand, Sprite handSprite, World world)
        {
            _hand = hand;
            _handSprite = handSprite;
            _world = world;

            _jointOffsets[0].X = 0.0f;
            _jointOffsets[0].Y = 0.1f;

            _jointOffsets[1].X = -0.1f;
            _jointOffsets[1].Y = -0.1f;

            _jointOffsets[2].X = 0.1f;
            _jointOffsets[2].Y = -0.1f;
        }

        public Virus GrabbedVirus()
        {
            return _grabbedVirus;
        }

        public void HandCollisions()
        {
            AABB leftHandAABB = GetHandAABB();

            if (_grabbedVirus == null)
            {

                if (float.IsNaN(_handCenter.X))
                    return;

                _world.QueryAABB(AABBVirusHandCollision, ref leftHandAABB);
                if (_grabbedVirus != null)
                {
                    _grabbedVirus.Grabbed();
                    CreateVirusSprings();
                }
            }
        }

        private FixedDistanceJoint[] _joints = new FixedDistanceJoint[3];
        private Vector2[] _jointOffsets = new Vector2[3];
        private void CreateVirusSprings()
        {
            if (_grabbedVirus.Body.Position.X == float.NaN || _handCenter.X == float.NaN)
                return;

            if (_joints[0] != null)
                RemoveVirusSprings();

            float length = 0.1f;

            for (int i = 0; i < _joints.Count(); i++)
            {
                _joints[i] = JointFactory.CreateFixedDistanceJoint(_world, _grabbedVirus.Body, Vector2.Zero, _handCenter + _jointOffsets[i]);
                _joints[i].Length = length;
                _joints[i].Frequency = 5.0f;
                _joints[i].DampingRatio = 1.5f;
            }
        }

        private void RemoveVirusSprings()
        {
            for (int i = 0; i < _joints.Count(); i++)
            {
                _world.RemoveJoint(_joints[i]);
                _joints[i] = null;
            }
        }

        private static readonly float BREAKING_POINT = 0.7f * 0.7f;
        public bool HandleVirusSprings()
        {
            bool broken = false;

            if (_grabbedVirus != null)
            {
                if (_grabbedVirus.IsAssimilating)
                {
                    DroppedVirus();
                    return true;
                }

                for (int i = 0; i < _joints.Count(); i++)
                {
                    _joints[i].WorldAnchorB = _handCenter + _jointOffsets[i];
                    if ((_joints[i].WorldAnchorA - _joints[i].WorldAnchorB).LengthSquared() > BREAKING_POINT)
                        broken = true;
                }

                if (broken)
                {
                    DroppedVirus();
                }
            }
            return broken;
        }

        private void DroppedVirus()
        {
            _grabbedVirus.Dropped();
            _grabbedVirus = null;
            _outerBodyWatch.Reset();
            RemoveVirusSprings();
        }

        public void OuterWallCollision()
        {
            _outerBodyWatch.Start();
            if (_outerBodyWatch.Elapsed.TotalSeconds > OUTER_BODY_CLOSED_TIME)
                _grabbedVirus.EnteringPlayerCell();
        }

        public void CenterCollision()
        {
            DroppedVirus();
        }

        private bool AABBVirusHandCollision(Fixture f)
        {
            Object o = f.Body.UserData;
            
            if (o is Virus)
            {
                Virus virus = (Virus)o;

                if (virus.IsConsumed == false && virus.IsGrabbed == false)
                    _grabbedVirus = (Virus)o;
            }
            return true;
        }

        private AABB GetHandAABB()
        {
            AABB handAABB;

            Vector2 adjustedPosition = _handSprite.Position;
            //adjustedPosition.X -= handSprite.Origin.X;
            //adjustedPosition.Y -= handSprite.Origin.Y;


            double cosA = Math.Cos(_handSprite.Rotation);
            double sinA = Math.Sin(_handSprite.Rotation);

            double ABScosA = (cosA < 0) ? -cosA : cosA;
            double ABSsinA = (sinA < 0) ? -sinA : sinA;

            float height = (float)(_handSprite.ScaledSize.X * ABSsinA + _handSprite.ScaledSize.Y * ABScosA);
            float width = (float)(_handSprite.ScaledSize.X * ABScosA + _handSprite.ScaledSize.Y * ABSsinA);

            if (_hand == Hand.LEFT)
            {
                //Subtraction in the adjust
                adjustedPosition.X -= (float)cosA * width / 2;
                adjustedPosition.Y -= (float)sinA * height / 2;

                _handCenter = ConvertUnits.ToSimUnits(adjustedPosition);
            }
            else
            {
                //Addition in the adjust
                adjustedPosition.X += (float)cosA * width / 2;
                adjustedPosition.Y += (float)sinA * height / 2;
                _handCenter = ConvertUnits.ToSimUnits(adjustedPosition);
            }

            adjustedPosition = ConvertUnits.ToSimUnits(adjustedPosition);
            height = ConvertUnits.ToSimUnits(height);
            width = ConvertUnits.ToSimUnits(width);

            handAABB = new AABB(adjustedPosition, width, height);

            return handAABB;
        }
    }
}
