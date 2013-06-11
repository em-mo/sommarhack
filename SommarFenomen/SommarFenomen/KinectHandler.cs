using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Kinect;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace SommarFenomen
{
    class KinectHandler
    {
        KinectSensor sensor = null;
        PlayWindow game;
        KinectStrategy kinectStrategy;
        bool running = true;
        
        Skeleton currentSkeleton;

        HandChecker leftHandChecker;
        HandChecker rightHandChecker;

        public KinectHandler(PlayWindow owner)
        {
            game = owner;
            kinectStrategy = (KinectStrategy)game.Player.Strategy;
            leftHandChecker = new HandChecker(kinectStrategy, Arm.Left, JointType.HandLeft);
            rightHandChecker = new HandChecker(kinectStrategy, Arm.Right, JointType.HandRight);

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
            regnStopwatch.Start();
        }

        public void run()
        {
            while (running)
            {
                if (sensor != null)
                    ProcessSkeletonFrame();
                else
                    System.Threading.Thread.Sleep(10);
            }
        }

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

            if (currentSkeleton != null && currentSkeleton.TrackingState != SkeletonTrackingState.NotTracked)
            {
                HandleArmAngles();

                HandleSwipes();
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
                    if (currentSkeleton != null && currentSkeleton.TrackingId != closestSkeleton.TrackingId)
                    {
                        leftHandChecker.Initialize(currentSkeleton);
                        rightHandChecker.Initialize(currentSkeleton);
                    }
                    this.currentSkeleton = closestSkeleton;
                    this.sensor.SkeletonStream.ChooseSkeletons(closestID); // Track this skeleton

                    
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
        
        /// <summary>
        /// Checks for hand movement by buffering skeletons and comparing them over time.
        /// Introduces a slight delay to movement reactions
        /// </summary>
        private class HandChecker
        {
            private const float FORCE_FACTOR = 1000;
            private const float ALTERNATE_FORCE = 8000;
            private const int BUFFER_LENGTH = 6;
            private readonly int START_POINT_OFFSET = BUFFER_LENGTH / 2;
            private const float MOVEMENT_THRESHOLD = 0.3f;

            private Arm arm;
            private JointType joint;
            private SkeletonPoint[] handPositions = new SkeletonPoint[BUFFER_LENGTH];
            private int handPositionsCounter;
            private int handPositionsHead;
            
            public HandChecker(KinectStrategy kinectStrategy, Arm arm, JointType joint)
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
                const int START = 2;
                const float ALTERNATE_THRESHOLD = 0.08f;
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
                if (diffVector.X < -ALTERNATE_THRESHOLD)
                    force.X = -ALTERNATE_FORCE * diffVector.X;
                else if (diffVector.X > ALTERNATE_THRESHOLD)
                    force.X = -ALTERNATE_FORCE * diffVector.X;

                if (diffVector.Y < -ALTERNATE_THRESHOLD)
                    force.Y = ALTERNATE_FORCE * diffVector.Y;
                else if (diffVector.Y > ALTERNATE_THRESHOLD)
                    force.Y = ALTERNATE_FORCE * diffVector.Y;

                handPositionsHead = (handPositionsHead + 1) % BUFFER_LENGTH;

                return force;
            }
        }

        #region regndans
        private bool inRegndans = false;
        private bool regndansHasFailed = false;
        private float armsToHeadDiff = 0;
        private Stopwatch armsEdgeStopwatch = new Stopwatch();
        private Stopwatch armsMidStopwatch = new Stopwatch();
        private Stopwatch startUpDelayStopwatch = new Stopwatch();

        private const int START_UP_DELAY = 700;
        private const float ARM_TO_HIP_THRESHOLD = 0.4f;
        private const float HANDS_TOGETHER = 0.55f;
        private const int EDGE_TIME = 1700;
        private const int MID_TIME = 1100;
        /// <summary>
        /// Checks for arms over head and movement of arms side to side, if yes then rain!
        /// 
        /// Waits until the hands are above a point between the head and shoulders then checks for movement.
        /// Uses timers in two different states to find movement, either hands to the side or hands in the mid.
        /// If the timers expire, the hands must be brought down to restart the regndans.
        /// 
        /// </summary>
        private bool CheckRegndans()
        {
            var headPositionY = currentSkeleton.Joints[JointType.ShoulderCenter].Position.Y +
                               (currentSkeleton.Joints[JointType.Head].Position.Y -
                                currentSkeleton.Joints[JointType.ShoulderCenter].Position.Y) * 0.5;
            // Hands over head
            if (currentSkeleton.Joints[JointType.HandRight].Position.Y > headPositionY &&
                currentSkeleton.Joints[JointType.HandLeft].Position.Y > headPositionY &&
                currentSkeleton.Joints[JointType.HandRight].Position.X - currentSkeleton.Joints[JointType.HandLeft].Position.X < HANDS_TOGETHER)
            {
                armsToHeadDiff = calculateArmsToHipDifferens();

                if (inRegndans)
                {
                    // Arms entering edge
                    if (Math.Abs(armsToHeadDiff) > ARM_TO_HIP_THRESHOLD && armsEdgeStopwatch.IsRunning == false)
                    {
                        armsMidStopwatch.Stop();
                        armsMidStopwatch.Reset();

                        armsEdgeStopwatch.Start();
                    }
                    // Arms leaving edge
                    else if (Math.Abs(armsToHeadDiff) < ARM_TO_HIP_THRESHOLD && armsEdgeStopwatch.IsRunning == true)
                    {
                        armsEdgeStopwatch.Stop();
                        armsEdgeStopwatch.Reset();

                        armsMidStopwatch.Start();
                    }
                    // Arms at edge
                    else if (armsEdgeStopwatch.IsRunning)
                    {
                        if (armsEdgeStopwatch.ElapsedMilliseconds > EDGE_TIME)
                        {
                            StopRegndans();
                            regndansHasFailed = true;
                            System.Console.WriteLine("edge");
                        }
                    }
                    // Arms in middle
                    else if (armsMidStopwatch.IsRunning == true)
                    {
                        if (armsMidStopwatch.ElapsedMilliseconds > MID_TIME)
                        {
                            StopRegndans();
                            regndansHasFailed = true;
                            System.Console.WriteLine("Mid");
                        }
                    }
                }
                // Hands in mid before regndans
                // Start after a delay
                else if (Math.Abs(armsToHeadDiff) < ARM_TO_HIP_THRESHOLD && !regndansHasFailed)
                {
                    if (startUpDelayStopwatch.IsRunning == false)
                        startUpDelayStopwatch.Start();

                    if (startUpDelayStopwatch.ElapsedMilliseconds > START_UP_DELAY)
                    {
                        startUpDelayStopwatch.Stop();
                        startUpDelayStopwatch.Reset();
                        StartRegndans();
                    }
                }
                else
                {
                    if (startUpDelayStopwatch.IsRunning)
                    {
                        startUpDelayStopwatch.Stop();
                        startUpDelayStopwatch.Reset();
                    }
                }
            }
                //Hands down or apart
            else
            {
                StopRegndans();
                regndansHasFailed = false;
            }
            return inRegndans;
        }

        private void StopRegndans()
        {
            if (armsEdgeStopwatch.IsRunning)
            {
                armsEdgeStopwatch.Stop();
                armsEdgeStopwatch.Reset();
            }

            if (armsMidStopwatch.IsRunning)
            {
                armsMidStopwatch.Stop();
                armsMidStopwatch.Reset();
            }

            if (startUpDelayStopwatch.IsRunning)
            {
                startUpDelayStopwatch.Stop();
                startUpDelayStopwatch.Reset();
            }

            inRegndans = false;
        }

        private void StartRegndans()
        {
            armsEdgeStopwatch.Start();
            inRegndans = true;
        }

        private const float EXPECTED_NOISE = 0.015f;
        private const int ARMS_MOVEMENT_RESET_THRESHOLD = 2;
        private int armsMovementResetCounter = 0;

        /// <summary>
        /// Produces a float depending on the hands position relative to the head.
        /// </summary>
        /// <returns></returns>
        private float calculateArmsToHipDifferens()
        {
            float leftHandDifferens;
            float rightHandDifferens;

            leftHandDifferens = currentSkeleton.Joints[JointType.HandLeft].Position.X - currentSkeleton.Joints[JointType.HipCenter].Position.X;
            rightHandDifferens = currentSkeleton.Joints[JointType.HandRight].Position.X - currentSkeleton.Joints[JointType.HipCenter].Position.X;

            return leftHandDifferens + rightHandDifferens;

        }
        #endregion
    }
}
