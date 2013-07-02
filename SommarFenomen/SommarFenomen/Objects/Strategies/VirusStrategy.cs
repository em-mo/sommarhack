using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Collision;
using SommarFenomen.Util;
using System.Diagnostics;

namespace SommarFenomen.Objects.Strategies
{
    class VirusStrategy : Strategy
    {
        private static readonly float ACCELERATION = 0.8f;
        private static readonly float NOISE_ACCELERATION = 5;
        private ActiveGameObject _target;
        private float _targetDistance;

        private double timer = 10;
        
        public VirusStrategy()
        {

        }

        public override Microsoft.Xna.Framework.Vector2 GetAcceleration()
        {
            Vector2 moveDirection = Vector2.Zero;
            if (_target != null)
            {
                moveDirection = (_target.Position - Owner.Position);
                moveDirection.Normalize();
            }
            Vector2 randomNoiseDirection = new Vector2((float)(Shared.Random.NextDouble() - 0.5), (float)(Shared.Random.NextDouble() - 0.5));
            randomNoiseDirection.Normalize();
            float randomSpeed = (float)Shared.Random.NextDouble() * NOISE_ACCELERATION;

            return moveDirection * ACCELERATION + randomNoiseDirection * randomSpeed;
        }

        public override void Update(GameTime gameTime)
        {
            PlayWindow playWindow = Owner.PlayWindow;
            timer += gameTime.ElapsedGameTime.TotalSeconds;
            if (_target != null)
                _targetDistance = (Owner.Position - _target.Position).Length();
            else
                _targetDistance = float.MaxValue;

            if (timer > 5 && _targetDistance > 2)
            {
                FindTarget(Owner.PlayWindow.World);
                timer = 0;
            }
        }

        private ActiveGameObject closestTarget;
        private float closestDistance;
        private bool targetFound;
        private void FindTarget(World world)
        {
            Vector2 currentPosition = Owner.Position;
            closestTarget = Owner.PlayWindow.GoodCellList.First();
            closestDistance = (currentPosition - closestTarget.Position).LengthSquared();
            float boxSideStep = 2;
            targetFound = false;
            while (boxSideStep < 64 && targetFound == false)
            {
                AABB boundingBox = new AABB(ConvertUnits.ToSimUnits(currentPosition), boxSideStep, boxSideStep);

                world.QueryAABB(AABBCollision, ref boundingBox);

                boxSideStep *= 2;
            }
            _target = closestTarget;
        }

        private bool AABBCollision(Fixture f)
        {
            Object o = f.Body.UserData;
            if (o == null)
                return true;

            if (o is GoodCell)
            {
                GoodCell cell = (GoodCell)o;
                float distance = (Owner.Position - cell.Position).LengthSquared();

                if (closestDistance > distance)
                {
                    closestDistance = distance;
                    closestTarget = cell;
                }
                targetFound = true;
            }

            return true;
        }
    }

    class VirusAssimilateStrategy : Strategy
    {
        private static readonly float ACCELERATION = 1200.0f;
        public ActiveGameObject Target { get; set; }

        public VirusAssimilateStrategy(ActiveGameObject target)
        {
            Target = target;
        }

        public override Vector2 GetAcceleration()
        {
            if (Target != null)
            {
                Owner.Body.ResetDynamics();
                Vector2 moveDirection;
                float distance;
                moveDirection = (Target.Position - Owner.Position);
                distance = ConvertUnits.ToSimUnits(moveDirection.Length());
                moveDirection.Normalize();

                return moveDirection * distance * ACCELERATION;
            }
            else 
                return Vector2.Zero;
        }
    }
}
