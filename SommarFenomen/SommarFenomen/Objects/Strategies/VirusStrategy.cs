using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Collision;

namespace SommarFenomen.Objects.Strategies
{
    class VirusStrategy : Strategy
    {
        private static readonly float ACCELERATION = 10;
        private static readonly float NOISE_ACCELERATION = 5;
        private ActiveGameObject _target;
        private float _targetDistance;

        private double timer = 10;
        
        public VirusStrategy()
        {

        }

        public override Microsoft.Xna.Framework.Vector2 GetAcceleration()
        {
            Vector2 randomNoiseDirection = new Vector2((float)Shared.Random.NextDouble(), (float)Shared.Random.NextDouble());
            randomNoiseDirection.Normalize();
            Vector2 moveDirection = (_target.Position - Owner.Position);
            moveDirection.Normalize();

            float randomSpeed = (float)Shared.Random.NextDouble() * NOISE_ACCELERATION;

            return moveDirection * ACCELERATION + randomNoiseDirection * randomSpeed;
        }

        public override void Update(GameTime gameTime)
        {
            PlayWindow playWindow = Owner.PlayWindow;
            _targetDistance = (Owner.Position - _target.Position).Length();

            if (timer > 5 && _targetDistance > 2)
                FindTarget(Owner.PlayWindow.World);
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
                AABB boundingBox = new AABB(currentPosition, boxSideStep, boxSideStep);

                world.QueryAABB(AABBCollision, ref boundingBox);

                boxSideStep *= 2;
            }
        }

        private bool AABBCollision(Fixture f)
        {
            Object o = f.Body.UserData.GetType();
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
}
