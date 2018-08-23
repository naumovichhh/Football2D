using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using FarseerPhysics.Fluids;
using Microsoft.Xna.Framework;

namespace Zevruk
{
    internal class PhysicsProcess
    {
        protected const float FieldWidth = 13.66f, FieldHeight = 7.68f, PlayerXPosition = 4,
            PlayerYPosition = 6.5f, GroundThickness = 0.69f, WallThickness = 0.06f,
            PlayerThickness = 0.3f, PlayerBodyHeight = 0.6f, PlayerHeadRadius = 0.14f,
            BallRadius = 0.15f, BallDensity = 0.117f, BallFriction = 0.09f;

        protected Body ground, roof, leftWall, rightWall, playerLeft, playerRight, legLeft, legRight, goalLeft, goalRight, ball;
        protected World world = new World(new Vector2(0, 4.5f), new AABB() { LowerBound = new Vector2(-1, -1), UpperBound = new Vector2(14.66f, 12f) });
        protected RevoluteJoint jointLeft, jointRight;
        protected bool shouldAttack;
        protected Vector2 ballPreviousSpeed;
        protected PlayerContacts playerLeftContacts, playerRightContacts;
        protected PlayerTimers playerLeftTimers, playerRightTimers;
        protected float playerLeftLegForce, playerRightLegForce;
        protected PlayerSpeeds playerLeftSpeeds, playerRightSpeeds;
        protected bool scored;
        protected int leftPostRaiseCount, rightPostRaiseCount;
        protected BallContacts ballContacts;

        public PhysicsProcess(BallRebound ballRebound, bool shouldAttack)
        {
            this.shouldAttack = shouldAttack;
            this.ball = BodyFactory.CreateCircle(
                this.world,
                BallRadius,
                BallDensity,
                new Vector2(FieldWidth / 2, 2));
            this.ball.Friction = BallFriction;
            this.ball.Restitution = ballRebound == BallRebound.High ? 0.87f :
                                    ballRebound == BallRebound.Medium ? 0.6f : 0.4f;
            this.ball.BodyType = BodyType.Dynamic;
            this.ball.LinearVelocity = new Vector2(new Random().Next(-10, 11) / 8f, -2);
            this.ball.OnCollision += this.BallContacting;
            this.ball.OnSeparation += this.BallContacted;
            
            this.ground = BodyFactory.CreateRectangle(
                this.world,
                FieldWidth,
                GroundThickness,
                1,
                new Vector2(FieldWidth / 2, FieldHeight - 0.37f));
            this.ground.Friction = 1;

            this.leftWall = BodyFactory.CreateRectangle(
                this.world,
                WallThickness,
                FieldHeight,
                1,
                new Vector2(-WallThickness / 2, FieldHeight / 2));

            this.rightWall = BodyFactory.CreateRectangle(
                this.world,
                WallThickness,
                FieldHeight,
                1,
                new Vector2(FieldWidth + (WallThickness / 2), FieldHeight / 2));
            
            this.roof = BodyFactory.CreateRectangle(
                this.world,
                FieldWidth,
                WallThickness,
                1,
                new Vector2(FieldWidth / 2, -WallThickness / 2));
            
            PolygonShape playerBody = new PolygonShape(1.3f);
            this.playerLeft = BodyFactory.CreateRectangle(
                this.world,
                PlayerThickness,
                PlayerBodyHeight,
                1.3f,
                new Vector2(PlayerXPosition, PlayerYPosition));
            this.playerLeft.Friction = 2;
            this.playerLeft.Restitution = 0;
            this.playerLeft.FixedRotation = true;
            this.playerLeft.BodyType = BodyType.Dynamic;
            CircleShape playerHead = new CircleShape(PlayerHeadRadius, 1);
            playerHead.Position = new Vector2(0, -0.35f);
            this.playerLeft.CreateFixture(playerHead, "Head");
            this.playerLeft.FixtureList[0].UserData = "Body";
            this.playerLeft.FixtureList[0].Friction = 2;
            this.playerLeft.FixtureList[0].Restitution = 0;
            this.playerLeft.FixtureList[1].Friction = 2;
            this.playerLeft.FixtureList[1].Restitution = 0;
            this.playerLeft.OnCollision += this.PlayerLeftContacting;
            this.playerLeft.OnSeparation += this.PlayerLeftContacted;
            this.playerLeft.OnCollision += this.PlayersTackling;

            this.playerRight = BodyFactory.CreateRectangle(
                this.world,
                PlayerThickness,
                PlayerBodyHeight,
                1.3f,
                new Vector2(FieldWidth - PlayerXPosition, PlayerYPosition));
            this.playerRight.BodyType = BodyType.Dynamic;
            this.playerRight.FixedRotation = true;
            this.playerRight.CreateFixture(playerHead, "Head");
            this.playerRight.FixtureList[0].UserData = "Body";
            this.playerRight.FixtureList[0].Friction = 2;
            this.playerRight.FixtureList[0].Restitution = 0;
            this.playerRight.FixtureList[1].Friction = 10;
            this.playerRight.FixtureList[1].Restitution = 0;
            this.playerRight.OnCollision += this.PlayerRightContacting;
            this.playerRight.OnSeparation += this.PlayerRightContacted;

            this.goalLeft = BodyFactory.CreateBody(this.world, new Vector2(0.5f, 5.035f));
            CircleShape goalLeftBar = new CircleShape(0.075f, 1);
            goalLeftBar.Position = new Vector2(0.425f, 0);
            this.goalLeft.CreateFixture(goalLeftBar);
            this.goalLeft.FixtureList[0].Friction = 1;
            this.goalLeft.FixtureList[0].Restitution = 0;
            this.goalLeft.FixtureList[0].UserData = "Post";
            Vector2[] vertices = new Vector2[] { new Vector2(-0.5f, -0.07f), new Vector2(0.42f, -0.07f), new Vector2(0.42f, -0.068f), new Vector2(-0.5f, -0.068f) };
            PolygonShape goalLeftNet = new PolygonShape(new Vertices(vertices), 1);
            this.goalLeft.CreateFixture(goalLeftNet);
            this.goalLeft.FixtureList[1].Friction = 1;
            this.goalLeft.FixtureList[1].Restitution = 0;

            this.goalRight = BodyFactory.CreateBody(this.world, new Vector2(13.16f, 5.035f));
            CircleShape goalRightBar = new CircleShape(0.075f, 1);
            goalRightBar.Position = new Vector2(-0.425f, 0);
            this.goalRight.CreateFixture(goalRightBar);
            this.goalRight.FixtureList[0].Friction = 1;
            this.goalRight.FixtureList[0].Restitution = 0;
            this.goalRight.FixtureList[0].UserData = "Post";
            Vector2[] vertices2 = new Vector2[] { new Vector2(-0.42f, -0.07f), new Vector2(0.5f, -0.07f), new Vector2(0.5f, -0.068f), new Vector2(-0.42f, -0.068f) };
            PolygonShape goalRightNet = new PolygonShape(new Vertices(vertices2), 1);
            this.goalRight.CreateFixture(goalLeftNet);
            this.goalRight.FixtureList[1].Friction = 1;
            this.goalRight.FixtureList[1].Restitution = 0;

            this.legLeft = BodyFactory.CreateBody(this.world, new Vector2(this.playerLeft.Position.X, this.playerLeft.Position.Y + 0.2f));
            this.legLeft.BodyType = BodyType.Dynamic;
            Vector2[] vertices3 = new[] { new Vector2(0.12f, 0.03f), new Vector2(-0.07f, -0.06f), new Vector2(-0.03f, -0.15f) };
            PolygonShape leg = new PolygonShape(new Vertices(vertices3), 1.4f);
            this.legLeft.CreateFixture(leg);
            this.legLeft.FixtureList[0].Friction = 3;
            this.legLeft.FixtureList[0].Restitution = 0;
            this.jointLeft = JointFactory.CreateRevoluteJoint(this.world, this.playerLeft, new Vector2(0.05f, -0.4f), this.legLeft, new Vector2(0, -0.64f));
            this.jointLeft.LowerLimit = AngleConversion.DegreeToRadian(-18);
            this.jointLeft.UpperLimit = AngleConversion.DegreeToRadian(13);
            this.jointLeft.LimitEnabled = true;
            this.jointLeft.MotorEnabled = true;
            this.jointLeft.MaxMotorTorque = 0.08f;

            this.legRight = BodyFactory.CreateBody(this.world, new Vector2(this.playerRight.Position.X, this.playerRight.Position.Y + 0.2f));
            this.legRight.BodyType = BodyType.Dynamic;
            Vector2[] vertices4 = new[] { new Vector2(-0.12f, 0.03f), new Vector2(0.03f, -0.15f), new Vector2(0.07f, -0.06f) };
            PolygonShape leg2 = new PolygonShape(new Vertices(vertices4), 1.4f);
            this.legRight.CreateFixture(leg2);
            this.legRight.FixtureList[0].Friction = 3;
            this.legRight.FixtureList[0].Restitution = 0;
            this.jointRight = JointFactory.CreateRevoluteJoint(this.world, this.playerRight, new Vector2(-0.05f, -0.4f), this.legRight, new Vector2(0, -0.64f));
            this.jointRight.LowerLimit = AngleConversion.DegreeToRadian(-13);
            this.jointRight.UpperLimit = AngleConversion.DegreeToRadian(18);
            this.jointRight.LimitEnabled = true;
            this.jointRight.MotorEnabled = true;
            this.jointRight.MaxMotorTorque = 0.08f;

            this.playerLeftTimers.Defend = 12000;
            this.playerRightTimers.Defend = 12000;
        }

        public event Action RightConceded, LeftConceded, RightPostRaised, LeftPostRaised;
        public event Action<float> BallTackled, BallPostTackled;

        public enum BallRebound
        {
            Small, Medium, High
        }

        public Vector2 PlayerLeftPosition => this.playerLeft.Position;
        public Vector2 LegLeftPosition => this.legLeft.Position;
        public float LegLeftAngle => this.legLeft.Rotation;
        public Vector2 PlayerRightPosition => this.playerRight.Position;
        public Vector2 LegRightPosition => this.legRight.Position;
        public float LegRightAngle => this.legRight.Rotation;
        public Vector2 BallPosition => this.ball.Position;
        public float BallAngle => this.ball.Rotation;

        public void Handle(float iterationDuration, PlayersInput input)
        {
            if (iterationDuration > 35)
                iterationDuration = 35;
            this.BallHandle();
            this.LegsHandleBeforeStep();
            this.PlayerLeftOperate(input, iterationDuration);
            this.PlayerRightOperate(input, iterationDuration);
            this.world.Step(iterationDuration / 1000);
            this.LegsHandleAfterStep(iterationDuration);
            this.DetectGoal();
            this.CheckImpudentDefense(iterationDuration);
        }

        protected void CheckImpudentDefense(float iterationDuration)
        {
            if (this.scored || !this.shouldAttack)
                return;

            bool playerLeftDefends, playerRightDefends;

            if (this.playerLeft.Position.X < 4.24f && this.playerRight.Position.X * 1 > 4.24f)
                playerLeftDefends = true;
            else
                playerLeftDefends = false;

            if (this.playerRight.Position.X > 9.41f && this.playerLeft.Position.X * 1 < 9.41f)
                playerRightDefends = true;
            else
                playerRightDefends = false;

            if (playerLeftDefends && this.leftPostRaiseCount <= 3)
            {
                this.playerLeftTimers.Defend += iterationDuration;
                if (this.playerLeftTimers.Defend < 0)
                {
                    this.goalLeft.Position = new Vector2(this.goalLeft.Position.X, this.goalLeft.Position.Y - 0.5f);
                    ++this.leftPostRaiseCount;
                    this.playerLeftTimers.Defend = 12000;
                    this.LeftPostRaised?.Invoke();
                }
            }

            if (playerRightDefends && this.rightPostRaiseCount <= 3)
            {
                this.playerRightTimers.Defend += iterationDuration;
                if (this.playerRightTimers.Defend < 0)
                {
                    this.goalRight.Position = new Vector2(this.goalRight.Position.X, this.goalRight.Position.Y - 0.5f);
                    ++this.rightPostRaiseCount;
                    this.playerRightTimers.Defend = 12000;
                    this.RightPostRaised?.Invoke();
                }
            }
        }

        protected void DetectGoal()
        {
            if (this.scored)
                return;

            if (this.ball.Position.Y > this.goalLeft.Position.Y)
            {
                if (this.ball.Position.X + this.ball.FixtureList[0].Shape.Radius < 0.85f)
                {
                    this.scored = true;
                    this.LeftConceded?.Invoke();
                }
            }

            if (this.ball.Position.Y > this.goalRight.Position.Y)
            {
                if (this.ball.Position.X - this.ball.FixtureList[0].Shape.Radius > 12.81f)
                {
                    this.scored = true;
                    this.RightConceded?.Invoke();
                }
            }
        }

        protected void LegsHandleBeforeStep()
        {
            float legLeftCoef = AngleConversion.RadianToDegree(this.legLeft.Rotation) > -2 ? 1 : (float)System.Math.Pow(System.Math.Sin(AngleConversion.DegreeToRadian(92) + this.legLeft.Rotation), 15),
              legRightCoef = AngleConversion.RadianToDegree(this.legRight.Rotation) < 2 ? 1 : (float)System.Math.Pow(System.Math.Sin(AngleConversion.DegreeToRadian(92) - this.legRight.Rotation), 15);
            this.legLeft.ApplyTorque(legLeftCoef);
            this.legRight.ApplyTorque(-legRightCoef);
            this.playerLeftSpeeds.BeforeStep = this.playerLeft.LinearVelocity.X;
            this.playerRightSpeeds.BeforeStep = this.playerRight.LinearVelocity.X;
        }

        protected void LegsHandleAfterStep(float iterationDuration)
        {
            this.playerLeftSpeeds.AfterStep = this.playerLeft.LinearVelocity.X;
            this.playerRightSpeeds.AfterStep = this.playerRight.LinearVelocity.X;
            float dx1, dx2;
            dx1 = (this.playerLeftSpeeds.AfterStep - this.playerLeftSpeeds.BeforeStep) / iterationDuration;
            dx2 = (this.playerRightSpeeds.AfterStep - this.playerRightSpeeds.BeforeStep) / iterationDuration;

            if (dx1 > 20)
                dx1 = 20;
            else if (dx1 < -20)
                dx1 = -20;

            if (dx2 > 20)
                dx2 = 20;
            else if (dx2 < -20)
                dx2 = -20;

            this.legLeft.ApplyForce(new Vector2(dx1 * this.legLeft.Mass, 0), this.legLeft.Position);
            this.legRight.ApplyForce(new Vector2(dx2 * this.legRight.Mass, 0), this.legRight.Position);
        }

        protected void PlayerLeftOperate(PlayersInput input, float iterationDuration)
        {
            if (this.playerLeftTimers.Jump >= 0)
                this.playerLeftTimers.Jump -= iterationDuration;

            Vector2 velocity;
            velocity = this.playerLeft.LinearVelocity;
            if (input.PlayerLeftLeft && (velocity.X > -2.3))
            {
                this.playerLeft.ApplyForce(new Vector2(-11, 0), this.playerLeft.Position);
            }

            if (input.PlayerLeftRight && (velocity.X < 2.3))
            {
                this.playerLeft.ApplyForce(new Vector2(11, 0), this.playerLeft.Position);
            }

            if (this.PlayerLeftCanJump() && (this.playerLeftTimers.Jump < 0))
            {
                if (input.PlayerLeftJump)
                {
                    this.playerLeft.ApplyLinearImpulse(new Vector2(0, -0.72f), this.playerLeft.Position);
                    this.playerLeftTimers.Jump = 100;
                }
            }

            if (this.playerLeftLegForce < -0.1f)
                this.playerLeftLegForce *= 0.97f;

            if (input.PlayerLeftKick && this.legLeft.AngularVelocity > -System.Math.PI * 9)
                this.playerLeftLegForce = -1.5f;

            if (this.playerLeftLegForce < -0.1f)
            {
                this.legLeft.ApplyTorque(this.playerLeftLegForce);
            }
        }

        protected void PlayerRightOperate(PlayersInput input, float iterationDuration)
        {
            if (this.playerRightTimers.Jump >= 0)
                this.playerRightTimers.Jump -= iterationDuration;

            Vector2 velocity;
            velocity = this.playerRight.LinearVelocity;
            if (input.PlayerRightLeft && (velocity.X > -2.3))
            {
                this.playerRight.ApplyForce(new Vector2(-11, 0), this.playerRight.Position);
            }

            if (input.PlayerRightRight && (velocity.X < 2.3))
            {
                this.playerRight.ApplyForce(new Vector2(11, 0), this.playerRight.Position);
            }

            if (this.PlayerRightCanJump() && (this.playerRightTimers.Jump < 0))
            {
                if (input.PlayerRightJump)
                {
                    this.playerRight.ApplyLinearImpulse(new Vector2(0, -0.72f), this.playerRight.Position);
                    this.playerRightTimers.Jump = 100;
                }
            }

            if (this.playerRightLegForce > 0.1f)
                this.playerRightLegForce *= 0.97f;

            if (input.PlayerRightKick && this.legRight.AngularVelocity < System.Math.PI * 9)
                this.playerRightLegForce = 1.5f;

            if (this.playerRightLegForce > 0.1f)
            {
                this.legRight.ApplyTorque(this.playerRightLegForce);
            }
        }

        protected bool PlayerLeftCanJump()
        {
            if (this.playerLeft.LinearVelocity.Y < -3)
                return false;

            if (this.playerLeftContacts.WithGround)
                return true;

            if (this.playerLeftContacts.WithGoal)
            {
                if ((this.playerLeft.Position.X < 1.15 || this.playerLeft.Position.X > 12.51)
                            && (this.playerLeft.Position.Y < 4.75))
                {
                    return true;
                }
            }

            if ((this.playerLeftContacts.WithAnotherPlayer || this.playerRightContacts.WithAnotherPlayer) && this.playerRightContacts.WithGround)
                return true;

            return false;
        }

        protected bool PlayerRightCanJump()
        {
            if (this.playerRight.LinearVelocity.Y < -3)
                return false;

            if (this.playerRightContacts.WithGround)
                return true;

            if (this.playerRightContacts.WithGoal)
            {
                if ((this.playerRight.Position.X < 1.15 || this.playerRight.Position.X > 12.51)
                            && (this.playerRight.Position.Y < 4.75))
                {
                    return true;
                }
            }

            if ((this.playerLeftContacts.WithAnotherPlayer || this.playerRightContacts.WithAnotherPlayer) && this.playerLeftContacts.WithGround)
                return true;

            return false;
        }

        protected void BallContacted(Fixture fixtureA, Fixture fixtureB)
        {
            Body bodyA = fixtureA.Body, bodyB = fixtureB.Body;
            float ballTackleSpeed;
            Body anotherBody;

            if (bodyA == this.ball)
                anotherBody = bodyB;
            else
                anotherBody = bodyA;

            if (anotherBody == this.playerLeft)
            {
                this.ballContacts.WithPlayerLeft = false;
            }

            if (anotherBody == this.playerRight)
            {
                this.ballContacts.WithPlayerRight = false;
            }

            if ((ballTackleSpeed = this.GetBallCollisionSpeed()) > 0.02f)
            {
                if (fixtureA.UserData as string == "Post" || fixtureB.UserData as string == "Post")
                    this.BallPostTackled?.Invoke(ballTackleSpeed);
                else
                    this.BallTackled?.Invoke(ballTackleSpeed);
            }
        }

        protected bool BallContacting(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            Body anotherBody;
            if (fixtureA.Body == this.ball)
                anotherBody = fixtureB.Body;
            else
                anotherBody = fixtureA.Body;

            if (anotherBody == this.playerLeft || anotherBody == this.playerRight)
            {
                if (anotherBody == this.playerLeft)
                {
                    this.ballContacts.WithPlayerLeft = true;
                    if (this.ballContacts.WithPlayerRight)
                        contact.Friction = 0;
                }
                else
                {
                    this.ballContacts.WithPlayerRight = true;
                    if (this.ballContacts.WithPlayerLeft)
                        contact.Friction = 0;
                }

                if ((string)fixtureA.UserData == "Head" || (string)fixtureB.UserData == "Head")
                {
                    contact.Restitution *= 1.2f;
                }
                else
                {
                    contact.Restitution *= 0.6f;
                }
            }

            return true;
        }

        protected bool PlayerLeftContacting(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            Body anotherBody;
            if (fixtureA.Body == this.playerLeft)
                anotherBody = fixtureB.Body;
            else
                anotherBody = fixtureA.Body;

            if (anotherBody == this.ground)
            {
                this.playerLeftContacts.WithGround = true;
            }
            else if (anotherBody == this.playerRight)
            {
                this.playerLeftContacts.WithAnotherPlayer = true;
            }
            else if (anotherBody == this.goalLeft || anotherBody == this.goalRight)
            {
                this.playerLeftContacts.WithGoal = true;
            }

            return true;
        }

        protected void PlayerLeftContacted(Fixture fixtureA, Fixture fixtureB)
        {
            Body anotherBody;
            if (fixtureA.Body == this.playerLeft)
                anotherBody = fixtureB.Body;
            else
                anotherBody = fixtureA.Body;

            if (anotherBody == this.ground)
            {
                this.playerLeftContacts.WithGround = false;
            }
            else if (anotherBody == this.playerRight)
            {
                this.playerLeftContacts.WithAnotherPlayer = false;
            }
            else if (anotherBody == this.goalLeft || anotherBody == this.goalRight)
            {
                this.playerLeftContacts.WithGoal = false;
            }
        }

        protected bool PlayerRightContacting(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            Body anotherBody;
            if (fixtureA.Body == this.playerRight)
                anotherBody = fixtureB.Body;
            else
                anotherBody = fixtureA.Body;

            if (anotherBody == this.ground)
            {
                this.playerRightContacts.WithGround = true;
            }
            else if (anotherBody == this.playerLeft)
            {
                this.playerRightContacts.WithAnotherPlayer = true;
            }
            else if (anotherBody == this.goalLeft || anotherBody == this.goalRight)
            {
                this.playerRightContacts.WithGoal = true;
            }

            return true;
        }

        protected void PlayerRightContacted(Fixture fixtureA, Fixture fixtureB)
        {
            Body anotherBody;
            if (fixtureA.Body == this.playerRight)
                anotherBody = fixtureB.Body;
            else
                anotherBody = fixtureA.Body;

            if (anotherBody == this.ground)
            {
                this.playerRightContacts.WithGround = false;
            }
            else if (anotherBody == this.playerLeft)
            {
                this.playerRightContacts.WithAnotherPlayer = false;
            }
            else if (anotherBody == this.goalLeft || anotherBody == this.goalRight)
            {
                this.playerRightContacts.WithGoal = false;
            }
        }

        protected bool PlayersTackling(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            if (this.playerRight == fixtureB.Body || this.playerRight == fixtureA.Body)
            {
                if (System.Math.Abs(fixtureA.Body.Position.Y - fixtureB.Body.Position.Y) < 0.7f)
                    contact.Friction = 0;
                else
                    contact.Friction = 5;
            }

            return true;
        }

        protected float GetBallCollisionSpeed()
        {
            double alpha, beta, gamma, omega;
            alpha = System.Math.Atan2(this.ballPreviousSpeed.X, this.ballPreviousSpeed.Y);
            beta = System.Math.Atan2(-this.ball.LinearVelocity.X, -this.ball.LinearVelocity.Y);
            if (System.Math.Abs(alpha - beta) > AngleConversion.DegreeToRadian(165))
                beta = -beta;

            gamma = (alpha + beta) / 2;
            omega = System.Math.Abs(gamma - beta);
            if (omega >= AngleConversion.DegreeToRadian(85))
                return 0;

            float previousFullSpeed = (float)System.Math.Sqrt(this.ballPreviousSpeed.X * this.ballPreviousSpeed.X + this.ballPreviousSpeed.Y * this.ballPreviousSpeed.Y);
            return (float)System.Math.Cos(omega) * previousFullSpeed;
        }

        protected void BallHandle()
        {
            float sinus, cosinus, x, y;
            x = this.ball.LinearVelocity.X;
            y = this.ball.LinearVelocity.Y;

            float Abs(float n) => n >= 0 ? n : -n;

            if ((Abs(x) > 0.001f) || (Abs(y) > 0.001f))
            {
                sinus = y / (float)System.Math.Sqrt(y * y + x * x);
                cosinus = x / (float)System.Math.Sqrt(y * y + x * x);
                this.ball.ApplyLinearImpulse(new Vector2(this.ball.AngularVelocity * -sinus * 0.0000006f, this.ball.AngularVelocity * cosinus * 0.0000006f), this.ball.Position);
            }

            this.ballPreviousSpeed = this.ball.LinearVelocity;

            for (ContactEdge e = this.ball.ContactList; e != null; e = e.Next)
            {
                if (!e.Contact.IsTouching())
                    continue;

                Body anotherBody;
                if (e.Contact.FixtureA.Body == this.ball)
                    anotherBody = e.Contact.FixtureB.Body;
                else
                    anotherBody = e.Contact.FixtureA.Body;

                if (anotherBody == this.goalLeft)
                {
                    this.ball.ApplyForce(new Vector2(0.005f, 0), this.ball.Position);
                }

                if (anotherBody == this.goalRight)
                {
                    this.ball.ApplyForce(new Vector2(-0.005f, 0), this.ball.Position);
                }

                if (anotherBody == this.ground)
                {
                    if (this.ball.AngularVelocity * BallRadius * (this.ball.AngularVelocity > 0 ? 1 : -1) >
                            0.85f * this.ball.LinearVelocity.X * (this.ball.LinearVelocity.X > 0 ? 1 : -1))
                    {
                        this.ball.AngularVelocity = this.ball.AngularVelocity * 0.95f;
                    }
                }
            }
        }

        protected struct PlayerContacts
        {
            public bool WithGround { get; set; }
            public bool WithAnotherPlayer { get; set; }
            public bool WithGoal { get; set; }
        }

        protected struct PlayerTimers
        {
            public float Jump { get; set; }
            public float Kick { get; set; }
            public float Defend { get; set; }
        }

        protected struct PlayerSpeeds
        {
            public float BeforeStep { get; set; }
            public float AfterStep { get; set; }
        }

        protected struct BallContacts
        {
            public bool WithPlayerLeft { get; set; }
            public bool WithPlayerRight { get; set; }
        }
    }
}
