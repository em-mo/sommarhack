using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Kinect;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using SommarFenomen.Util;

namespace SommarFenomen
{
    class KinectHandler
    {
        KinectSensor sensor = null;
        PlayWindow game;
        KinectStrategy kinectStrategy;
        bool running = true;
        
        Skeleton currentSkeleton;

        Stopwatch startDelayTimer = new Stopwatch();
        private static readonly double START_DELAY = 0.5;

        HandChecker leftHandChecker;
        HandChecker rightHandChecker;

        public event Action IdleRestart;

        public KinectHandler(PlayWindow owner)
        {
            game = owner;
            game.StartingNewGame += OnNewGame;
            kinectStrategy = (KinectStrategy)game.Player.Strategy;
            leftHandChecker = new HandChecker(Arm.Left, JointType.HandLeft);
            rightHandChecker = new HandChecker(Arm.Right, JointType.HandRight);

            FindSensor();
        }

        public void run()
        {
            while (running)
            {
                if (sensor != null)
                    ProcessSkeletonFrame();
                else
                {
                    FindSensor();
                    System.Threading.Thread.Sleep(10);
                }
            }
        }

        private void FindSensor()
        {
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor)
            {
                TransformSmoothParameters smoothingParam = new TransformSmoothParameters();
                {
                    smoothingParam.Smoothing = 0.5f;
                    smoothingParam.Correction = 0.1f;
                    smoothingParam.Prediction = 0.5f;
                    smoothingParam.JitterRadius = 0.07f;
                    smoothingParam.MaxDeviationRadius = 0.1f;
                };

                // Turn on the skeleton stream to receive skeleton frames
                this.sensor.SkeletonStream.Enable(smoothingParam);

                // Start the sensor!
                try
                {
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }
        }

        private enum KinectStates
        {
            IDLE, WAITING, RUNNING
        }
        private KinectStates currentState = KinectStates.RUNNING;
        private Stopwatch _restartWatch = new Stopwatch();
        private void ProcessSkeletonFrame()
        {
            Skeleton[] skeletons = new Skeleton[0];

            using (SkeletonFrame skeletonFrame = sensor.SkeletonStream.OpenNextFrame(100))
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                    skeletonFrame.Dispose();
                }
            }

            TrackClosestSkeleton(skeletons);

            if (currentSkeleton != null && currentSkeleton.TrackingState == SkeletonTrackingState.Tracked)
            {
                if (_restartWatch.IsRunning)
                    _restartWatch.Reset();

                HandleArmAngles();

                if (startDelayTimer.Elapsed.TotalSeconds > START_DELAY)
                    HandleSwipes();
                else
                    UpdateHandCheckers();
            }
            else
            {
                _restartWatch.Start();
                if (_restartWatch.Elapsed.TotalSeconds > 60)
                {
                    _restartWatch.Restart();
                    if (IdleRestart != null)
                        IdleRestart();
                }
            }
        }

        Stopwatch regnStopwatch = new Stopwatch();

        private void TrackClosestSkeleton(Skeleton[] skeletons)
        {
            int closestID = 0;

            if (this.sensor != null && this.sensor.SkeletonStream != null)
            {
                if (!this.sensor.SkeletonStream.AppChoosesSkeletons)
                {
                    this.sensor.SkeletonStream.AppChoosesSkeletons = true; // Ensure AppChoosesSkeletons is set
                }

                float closestDistance = 10000f; // Start with a far enough distance
                Skeleton closestSkeleton = null;

                foreach (Skeleton skeleton in skeletons.Where(s => s.TrackingState != SkeletonTrackingState.NotTracked))
                {
                    if (skeleton.Position.Z < closestDistance)
                    {
                        closestID = skeleton.TrackingId;
                        closestDistance = skeleton.Position.Z;
                        closestSkeleton = skeleton;
                    }
                }

                if (closestID > 0)
                {
                    if (currentSkeleton == null || currentSkeleton.TrackingId != closestSkeleton.TrackingId)
                    {
                        startDelayTimer.Restart();
                        leftHandChecker.Initialize(closestSkeleton);
                        rightHandChecker.Initialize(closestSkeleton);
                    }
                    this.currentSkeleton = closestSkeleton;
                    this.sensor.SkeletonStream.ChooseSkeletons(closestID); // Track this skeleton


                }
                else
                {
                    this.currentSkeleton = null;
                    if (kinectStrategy != null)
                        kinectStrategy.CurrentAcceleration = Vector2.Zero;
                }
            }
        }

        private void HandleArmAngles()
        {
            float leftHumerusAngle = CalculateArmAngle(JointType.ShoulderLeft, JointType.ElbowLeft, Arm.Left);
            float leftUlnaAngle = CalculateArmAngle(JointType.ElbowLeft, JointType.WristLeft, Arm.Left);

            float rightHumerusAngle = CalculateArmAngle(JointType.ShoulderRight, JointType.ElbowRight, Arm.Right);
            float rightUlnaAngle = CalculateArmAngle(JointType.ElbowRight, JointType.WristRight, Arm.Right);

            game.Player.SetLeftArmRotation(leftHumerusAngle, leftUlnaAngle);
            game.Player.SetRightArmRotation(rightHumerusAngle, rightUlnaAngle);
        }
        
        private float CalculateArmAngle(JointType startJoint1, JointType endJoint1,
                             Arm arm)
        {
            Joint joint1 = currentSkeleton.Joints[startJoint1];
            Joint joint2 = currentSkeleton.Joints[endJoint1];

            Vector3 vector1 = new Vector3(joint2.Position.X - joint1.Position.X, joint2.Position.Y - joint1.Position.Y, joint2.Position.Z - joint1.Position.Z);
            Vector3 vector2;
            if (arm == Arm.Left)
                vector2 = new Vector3(-1, 0, joint2.Position.Z - joint1.Position.Z);
            else
                vector2 = new Vector3(1, 0, joint2.Position.Z - joint1.Position.Z);

            return (float)Utils.CalculateAngle(vector1, vector2);
        }

        /// <summary>
        /// Chooses algorithm
        /// </summary>
        private void HandleSwipes()
        {
            Vector2 force = Vector2.Zero;
            if (game.MovmentType)
            {
                force += rightHandChecker.CheckHand(currentSkeleton);
                force += leftHandChecker.CheckHand(currentSkeleton);
            }

            else
            {
                force += rightHandChecker.AlternateCheckHand(currentSkeleton);
                force += leftHandChecker.AlternateCheckHand(currentSkeleton);
            }
            kinectStrategy.CurrentAcceleration = force;
        }

        private void UpdateHandCheckers()
        {
            rightHandChecker.UpdateHandPositions(currentSkeleton);
            leftHandChecker.UpdateHandPositions(currentSkeleton);
        }
        
        /// <summary>
        /// Checks for hand movement by buffering skeletons and comparing them over time.
        /// Introduces a slight delay to movement reactions
        /// </summary>
        private class HandChecker
        {
            private const float FORCE_FACTOR = 50;
            private const float ALTERNATE_FORCE = 1000;
            private const int BUFFER_LENGTH = 6;
            private readonly int START_POINT_OFFSET = 1;
            private const float MOVEMENT_THRESHOLD = 0.1f;

            private Arm arm;
            private JointType joint;
            private SkeletonPoint[] handPositions = new SkeletonPoint[BUFFER_LENGTH];
            private int handPositionsCounter;
            private int handPositionsHead;
            
            public HandChecker(Arm arm, JointType joint)
            {
                this.arm = arm;
                this.joint = joint;
            }

            public void Initialize(Skeleton currentSkeleton)
            {
                for (int i = 0; i < BUFFER_LENGTH; i++)
                {
                    handPositions[i] = currentSkeleton.Joints[joint].Position;
                }
            }

            public Vector2 CheckHand(Skeleton currentSkeleton)
            {
                
                Vector2 force = Vector2.Zero;

                // Add new element to array                
                handPositions[handPositionsHead] = currentSkeleton.Joints[joint].Position;

                int handStart = (handPositionsHead - START_POINT_OFFSET) % BUFFER_LENGTH;
                if (handStart < 0)
                    handStart = BUFFER_LENGTH + handStart;

                // Get old value from array
                SkeletonPoint startPoint = handPositions[handStart];
                // Get current value
                SkeletonPoint endPoint = handPositions[handPositionsHead];

                Vector2 movemetVector = new Vector2(endPoint.X - startPoint.X, endPoint.Y - startPoint.Y);
                // Check thresholds for X and Y
                if (movemetVector.X < -MOVEMENT_THRESHOLD)
                    force.X = FORCE_FACTOR;
                else if (movemetVector.X > MOVEMENT_THRESHOLD)
                    force.X = -FORCE_FACTOR;
                if (movemetVector.Y < -MOVEMENT_THRESHOLD)
                    force.Y = -FORCE_FACTOR;
                else if (movemetVector.Y > MOVEMENT_THRESHOLD)
                    force.Y = FORCE_FACTOR;

                handPositionsHead = (handPositionsHead + 1) % BUFFER_LENGTH;

                return force;
            }

            public Vector2 AlternateCheckHand(Skeleton currentSkeleton)
            {
                const int START = 1;
                const float ALTERNATE_THRESHOLD = 0.03f;
                Vector2 force = Vector2.Zero;

                // Add new element to array                
                handPositions[handPositionsHead] = currentSkeleton.Joints[joint].Position;

                Vector2 diffVector = Vector2.Zero;
                int handStart = (handPositionsHead - START) % BUFFER_LENGTH;
                if (handStart < 0)
                    handStart = BUFFER_LENGTH + handStart;

                int handEnd = (handStart + 1) % BUFFER_LENGTH;
                // Sum the latest differences and take the average
                for (int i = 1; i <= START; i++)
                {
                    diffVector.X += handPositions[handEnd].X;
                    diffVector.Y += handPositions[handEnd].Y;
                    diffVector.X -= handPositions[handStart].X;
                    diffVector.Y -= handPositions[handStart].Y;

                    handStart = (handStart + 1) % BUFFER_LENGTH;
                    handEnd = (handEnd + 1) % BUFFER_LENGTH;
                }
                //Average
                diffVector /= START;

                // Check thresholds for X and Y
                if (Math.Abs(diffVector.X) > ALTERNATE_THRESHOLD)
                    force.X = GetForce(-diffVector.X);

                if (Math.Abs(diffVector.Y) > ALTERNATE_THRESHOLD)
                    force.Y = GetForce(diffVector.Y);

                handPositionsHead = (handPositionsHead + 1) % BUFFER_LENGTH;

                return force;
            }

            private float GetForce(float vectorComponent)
            {
                vectorComponent *= (vectorComponent < 0) ? -vectorComponent : vectorComponent;
                return vectorComponent * ALTERNATE_FORCE;
            }

            public void UpdateHandPositions(Skeleton currentSkeleton)
            {
                handPositions[handPositionsHead] = currentSkeleton.Joints[joint].Position;
                handPositionsHead = (handPositionsHead + 1) % BUFFER_LENGTH;
            }
        }

        public bool HasSkeleton()
        {
            return currentSkeleton != null;
        }

        private void OnNewGame()
        {
            kinectStrategy = (KinectStrategy)game.Player.Strategy;
        }
    }
}
