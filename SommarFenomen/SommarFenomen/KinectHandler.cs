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

        private const float SWIPE_INITIALIZE_VALUE = 0F;
        private const float SWIPE_DELTA_QOTIENT = 1.7F;
        private const float SWIPE_THRESHOLD = 0.25F;
        
        //Right hand DOWN
        private float right_DownDeltaBuffer;
        private float right_DownPreviousPosition;
        //Right hand UP
        private float right_UpDeltaBuffer;
        private float right_UpPreviousPosition;
        //RightHand LEFT
        private float right_LeftDeltaBuffer;
        private float right_LeftPreviousPosition;
        //RightHand RIGHT
        private float right_RightDeltaBuffer;
        private float right_RightPreviousPosition;


        //Left hand DOWN
        private float left_DownDeltaBuffer;
        private float left_DownPreviousPosition;
        //Left hand UP
        private float left_UpDeltaBuffer;
        private float left_UpPreviousPosition;
        //LeftHand LEFT
        private float left_LeftDeltaBuffer;
        private float left_LeftPreviousPosition;
        //LeftHand left
        private float left_RightDeltaBuffer;
        private float left_RightPreviousPosition;

        KinectSensor sensor = null;
        GameWindow game;
        bool running = true;
        
        Skeleton currentSkeleton;

        HandChecker leftHandChecker;
        HandChecker rightHandChecker;

        public KinectHandler(GameWindow owner)
        {
            game = owner;

            leftHandChecker = new HandChecker(game, Arm.Left, JointType.HandLeft);
            rightHandChecker = new HandChecker(game, Arm.Right, JointType.HandRight);

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

                HandleRegndans();
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
                    this.currentSkeleton = closestSkeleton;
                    this.sensor.SkeletonStream.ChooseSkeletons(closestID); // Track this skeleton
                }
            }
        }

        private void HandleRegndans()
        {
            if (CheckRegndans() && !game.PlayerCloud.IsSick)
            {
                game.releaseRainDrops();
                game.StartNotCarrie();
            }
            else
                game.StopNotCarrie();
        }

        private void HandleArmAngles()
        {
            float leftHumerusAngle = CalculateArmAngle(JointType.ShoulderLeft, JointType.ElbowLeft, Arm.Left);
            float leftUlnaAngle = CalculateArmAngle(JointType.ElbowLeft, JointType.WristLeft, Arm.Left);

            float rightHumerusAngle = CalculateArmAngle(JointType.ShoulderRight, JointType.ElbowRight, Arm.Right);
            float rightUlnaAngle = CalculateArmAngle(JointType.ElbowRight, JointType.WristRight, Arm.Right);

            game.PlayerCloud.SetLeftArmRotation(leftHumerusAngle, leftUlnaAngle);
            game.PlayerCloud.SetRightArmRotation(rightHumerusAngle, rightUlnaAngle);
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
            if (game.MovmentType)
            {
                rightHandChecker.CheckHand(currentSkeleton);
                leftHandChecker.CheckHand(currentSkeleton);
            }

            else
            {
                if (CheckForRightHandSwipeUp(currentSkeleton.Joints[JointType.HandRight]))
                    game.SwipeUp(Arm.Right);
                if (CheckForRightHandSwipeDown(currentSkeleton.Joints[JointType.HandRight]))
                    game.SwipeDown(Arm.Right);
                if (CheckForRightHandSwipeToLeft(currentSkeleton.Joints[JointType.HandRight]))
                    game.SwipeLeft(Arm.Right);
                if (CheckForRightHandSwipeToRight(currentSkeleton.Joints[JointType.HandRight]))
                    game.SwipeRight(Arm.Right);


                if (CheckForLeftHandSwipeUp(currentSkeleton.Joints[JointType.HandLeft]))
                    game.SwipeUp(Arm.Left);
                if (CheckForLeftHandSwipeDown(currentSkeleton.Joints[JointType.HandLeft]))
                    game.SwipeDown(Arm.Left);
                if (CheckForLeftHandSwipeToLeft(currentSkeleton.Joints[JointType.HandLeft]))
                    game.SwipeLeft(Arm.Left);
                if (CheckForLeftHandSwipeToRight(currentSkeleton.Joints[JointType.HandLeft]))
                    game.SwipeRight(Arm.Left);
            }

        }
        
        /// <summary>
        /// Checks for hand movement by buffering skeletons and comparing them over time.
        /// Introduces a slight delay to movement reactions
        /// </summary>
        private class HandChecker
        {
            private const int BUFFER_LENGTH = 6;
            private readonly int START_POINT_OFFSET = BUFFER_LENGTH / 2;
            private const float MOVEMENT_THRESHOLD = 0.3f;

            private GameWindow game;
            private Arm arm;
            private JointType joint;
            private SkeletonPoint[] handPositions = new SkeletonPoint[BUFFER_LENGTH];
            private int handPositionsCounter;
            private int handPositionsHead;
            
            public HandChecker(GameWindow game, Arm arm, JointType joint)
            {
                this.game = game;
                this.arm = arm;
                this.joint = joint;
            }

            public void CheckHand(Skeleton currentSkeleton)
            {
                // Add new element to array                
                handPositions[handPositionsHead] = currentSkeleton.Joints[joint].Position;
                //Sub-optimal initialization
                if (handPositionsCounter < 10)
                    handPositionsCounter++;
                else
                {
                    // Get old value from array
                    SkeletonPoint startPoint = handPositions[Math.Abs((handPositionsHead - START_POINT_OFFSET) % BUFFER_LENGTH)];
                    // Get current value
                    SkeletonPoint endPoint = handPositions[handPositionsHead];

                    Vector2 movemetVector = new Vector2(endPoint.X - startPoint.X, endPoint.Y - startPoint.Y);

                    // Check thresholds for X and Y
                    if (movemetVector.X < -MOVEMENT_THRESHOLD)
                        game.AlternativeSwipe(arm, Direction.Left);
                    else if (movemetVector.X > MOVEMENT_THRESHOLD)
                        game.AlternativeSwipe(arm, Direction.Right);
                    if (movemetVector.Y < -MOVEMENT_THRESHOLD)
                        game.AlternativeSwipe(arm, Direction.Down);
                    else if (movemetVector.Y > MOVEMENT_THRESHOLD)
                        game.AlternativeSwipe(arm, Direction.Up);
                }
                handPositionsHead = (handPositionsHead + 1) % BUFFER_LENGTH;
            }
        }

        #region Righthand Swipe checks
        private DateTime rightHandcoolDown = DateTime.Now;
        public bool CheckForRightHandSwipeToLeft(Joint trackedJoint)
        {
            right_LeftDeltaBuffer /= SWIPE_DELTA_QOTIENT; 
            
            right_LeftDeltaBuffer += right_LeftPreviousPosition - trackedJoint.Position.X;

            right_LeftPreviousPosition = trackedJoint.Position.X;

            if (right_LeftDeltaBuffer > SWIPE_THRESHOLD && rightHandcoolDown < DateTime.Now)
            {
                right_LeftDeltaBuffer = 0F;
                rightHandcoolDown = DateTime.Now.AddSeconds(0.4);
                return true;
            }
            else
                return false;
        }

        public bool CheckForRightHandSwipeToRight(Joint trackedJoint)
        {
            right_RightDeltaBuffer /= SWIPE_DELTA_QOTIENT;

            right_RightDeltaBuffer += trackedJoint.Position.X - right_RightPreviousPosition;

            right_RightPreviousPosition = trackedJoint.Position.X;

            if (right_RightDeltaBuffer > SWIPE_THRESHOLD && rightHandcoolDown < DateTime.Now)
            {
                right_RightDeltaBuffer = 0F;
                rightHandcoolDown = DateTime.Now.AddSeconds(0.4);
                return true;
            }
            else
                return false;
        }

        public bool CheckForRightHandSwipeDown(Joint trackedJoint)
        {
            right_DownDeltaBuffer /= SWIPE_DELTA_QOTIENT;

            right_DownDeltaBuffer += right_DownPreviousPosition - trackedJoint.Position.Y;

            right_DownPreviousPosition = trackedJoint.Position.Y;
            
            if (right_DownDeltaBuffer > SWIPE_THRESHOLD && rightHandcoolDown < DateTime.Now)
            {
                right_DownDeltaBuffer = 0F;
                rightHandcoolDown = DateTime.Now.AddSeconds(0.4);
                return true;
            }
            else
                return false;
        }

        public bool CheckForRightHandSwipeUp(Joint trackedJoint)
        {
            right_UpDeltaBuffer /= SWIPE_DELTA_QOTIENT;
            
            right_UpDeltaBuffer += trackedJoint.Position.Y - right_UpPreviousPosition;

            right_UpPreviousPosition = trackedJoint.Position.Y;
            
            if (right_UpDeltaBuffer > SWIPE_THRESHOLD && rightHandcoolDown < DateTime.Now)
            {
                right_UpDeltaBuffer = 0F;
                rightHandcoolDown = DateTime.Now.AddSeconds(0.4);
                return true;
            }
            else
                return false;
        }
        #endregion

        #region Lefthand Swipe checks

        private DateTime leftHandcoolDown = DateTime.Now;
        public bool CheckForLeftHandSwipeToLeft(Joint trackedJoint)
        {
            left_LeftDeltaBuffer /= SWIPE_DELTA_QOTIENT;

            left_LeftDeltaBuffer += left_LeftPreviousPosition - trackedJoint.Position.X;

            left_LeftPreviousPosition = trackedJoint.Position.X;

            if (left_LeftDeltaBuffer > SWIPE_THRESHOLD && leftHandcoolDown < DateTime.Now)
            {
                left_LeftDeltaBuffer = 0F;
                rightHandcoolDown = DateTime.Now.AddSeconds(0.4);
                return true;
            }
            else
                return false;
        }

        public bool CheckForLeftHandSwipeToRight(Joint trackedJoint)
        {
            left_RightDeltaBuffer /= SWIPE_DELTA_QOTIENT;

            left_RightDeltaBuffer += trackedJoint.Position.X - left_RightPreviousPosition;

            left_RightPreviousPosition = trackedJoint.Position.X;

            if (left_RightDeltaBuffer > SWIPE_THRESHOLD && leftHandcoolDown < DateTime.Now)
            {
                left_RightDeltaBuffer = 0F;
                leftHandcoolDown = DateTime.Now.AddSeconds(0.4);
                return true;
            }
            else
                return false;
        }

        public bool CheckForLeftHandSwipeDown(Joint trackedJoint)
        {
            left_DownDeltaBuffer /= SWIPE_DELTA_QOTIENT;

            left_DownDeltaBuffer += left_DownPreviousPosition - trackedJoint.Position.Y;

            left_DownPreviousPosition = trackedJoint.Position.Y;

            if (left_DownDeltaBuffer > SWIPE_THRESHOLD && leftHandcoolDown < DateTime.Now)
            {
                left_DownDeltaBuffer = 0F;
                leftHandcoolDown = DateTime.Now.AddSeconds(0.4);
                return true;
            }
            else
                return false;
        }

        public bool CheckForLeftHandSwipeUp(Joint trackedJoint)
        {
            left_UpDeltaBuffer /= SWIPE_DELTA_QOTIENT;

            left_UpDeltaBuffer += trackedJoint.Position.Y - left_UpPreviousPosition;

            left_UpPreviousPosition = trackedJoint.Position.Y;

            if (left_UpDeltaBuffer > SWIPE_THRESHOLD && leftHandcoolDown < DateTime.Now)
            {
                left_UpDeltaBuffer = 0F;
                leftHandcoolDown = DateTime.Now.AddSeconds(0.4);
                return true;
            }
            else
                return false;
        }
        #endregion

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
