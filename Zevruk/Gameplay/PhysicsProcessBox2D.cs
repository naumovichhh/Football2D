using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Box2DX;
using Box2DX.Collision;
using Box2DX.Common;
using Box2DX.Dynamics;

namespace Zevruk
{
    internal class PhysicsProcessBox2D
    {
        private const float FieldWidth = 13.66f, FieldHeight = 7.68f, PlayerXPosition = 1.7f,
            PlayerYPosition = 6.5f, GroundThickness = 0.69f, WallThickness = 0.06f,
            PlayerThickness = 0.3f, PlayerBodyHeight = 0.6f, PlayerHeadRadius = 0.14f,
            BallRadius = 0.15f, BallDensity = 0.117f, BallFriction = 0.09f;

        private Body ground, roof, leftWall, rightWall, playerLeft, playerRight, legLeft, legRight, goalLeft, goalRight, ball;
        private World world = new World(new AABB() { LowerBound = new Vec2(-1, -1), UpperBound = new Vec2(14.66f, 12f) }, new Vec2(0, 4.5f), true);
        private Joint jointLeft, jointRight;
        private bool shouldAttack;
        private ZevrukContactListener listener = new ZevrukContactListener();
        private Vec2 ballPreviousSpeed;
        private PlayerContacts playerLeftContacts, playerRightContacts;
        private PlayerTimers playerLeftTimers, playerRightTimers;
        private float playerLeftLegForce, playerRightLegForce;
        private PlayerSpeeds playerLeftSpeeds, playerRightSpeeds;
        private bool scored;
        private int leftPostRaiseCount, rightPostRaiseCount;
        private BallContacts ballContacts;

        public PhysicsProcessBox2D(Settings.BallReboundType ballRebound, bool shouldAttack)
        {
            this.shouldAttack = shouldAttack;

            BodyDef ballDef = new BodyDef
            {
                AngularDamping = 0.01f,
                Position = new Vec2(FieldWidth / 2, 2)
            };
            this.ball = this.world.CreateBody(ballDef);
            this.ball.SetLinearVelocity(new Vec2(new Random().Next(-20, 21) / 8f, -2));
            CircleDef ballShapeDef = new CircleDef
            {
                Radius = BallRadius,
                Density = BallDensity,
                Friction = BallFriction,
                Restitution = ballRebound == Settings.BallReboundType.High ? 0.87f :
                              ballRebound == Settings.BallReboundType.Medium ? 0.6f : 0.4f
            };
            this.ball.CreateShape(ballShapeDef);
            this.ball.SetMassFromShapes();

            BodyDef groundDef = new BodyDef
            {
                Position = new Vec2(FieldWidth / 2, FieldHeight - 0.37f)
            };
            this.ground = this.world.CreateBody(groundDef);
            PolygonDef groundShapeDef = new PolygonDef
            {
                Friction = 1f
            };
            groundShapeDef.SetAsBox(FieldWidth / 2, GroundThickness / 2);
            this.ground.CreateShape(groundShapeDef);

            BodyDef leftWallDef = new BodyDef
            {
                Position = new Vec2(-WallThickness / 2, FieldHeight / 2)
            };
            this.leftWall = this.world.CreateBody(leftWallDef);
            PolygonDef leftWallShapeDef = new PolygonDef();
            leftWallShapeDef.SetAsBox(WallThickness / 2, FieldHeight / 2);
            this.leftWall.CreateShape(leftWallShapeDef);

            BodyDef rightWallDef = new BodyDef
            {
                Position = new Vec2(FieldWidth + (WallThickness / 2), FieldHeight / 2)
            };
            this.rightWall = this.world.CreateBody(rightWallDef);
            PolygonDef rightWallShapeDef = new PolygonDef();
            rightWallShapeDef.SetAsBox(WallThickness / 2, FieldHeight / 2);
            this.rightWall.CreateShape(rightWallShapeDef);

            BodyDef roofDef = new BodyDef
            {
                Position = new Vec2(FieldWidth / 2, -WallThickness / 2)
            };
            this.roof = this.world.CreateBody(roofDef);
            PolygonDef roofShapeDef = new PolygonDef();
            roofShapeDef.SetAsBox(FieldWidth / 2, WallThickness / 2);
            this.roof.CreateShape(roofShapeDef);

            BodyDef playerLeftDef = new BodyDef
            {
                FixedRotation = true,
                Position = new Vec2(PlayerXPosition, PlayerYPosition)
            };
            this.playerLeft = this.world.CreateBody(playerLeftDef);
            PolygonDef playerBody = new PolygonDef
            {
                Friction = 2,
                Density = 1.3f,
                Restitution = 0,
                UserData = "Body"
            };
            playerBody.SetAsBox(PlayerThickness / 2, PlayerBodyHeight / 2);
            this.playerLeft.CreateShape(playerBody);
            CircleDef playerHead = new CircleDef
            {
                Friction = 1,
                Density = 1,
                Restitution = 0,
                Radius = PlayerHeadRadius,
                LocalPosition = new Vec2(0, -0.35f),
                UserData = "Head"
            };
            this.playerLeft.CreateShape(playerHead);
            this.playerLeft.SetMassFromShapes();

            BodyDef playerRightDef = new BodyDef
            {
                FixedRotation = true,
                Position = new Vec2(FieldWidth - PlayerXPosition, PlayerYPosition)
            };
            this.playerRight = this.world.CreateBody(playerRightDef);
            this.playerRight.CreateShape(playerBody);
            this.playerRight.CreateShape(playerHead);
            this.playerRight.SetMassFromShapes();

            BodyDef goalLeftDef = new BodyDef
            {
                Position = new Vec2(0.5f, 5.035f)
            };
            this.goalLeft = this.world.CreateBody(goalLeftDef);
            CircleDef goalLeftBarDef = new CircleDef
            {
                Radius = 0.075f,
                LocalPosition = new Vec2(0.425f, 0),
                Friction = 1,
                Restitution = 0,
                UserData = "Post"
            };
            PolygonDef goalLeftLineDef = new PolygonDef
            {
                Friction = 1,
                Restitution = 0
            };
            goalLeftLineDef.SetAsBox(0.46f, 0.001f, new Vec2(-0.04f, -0.069f), 0);
            this.goalLeft.CreateShape(goalLeftBarDef);
            this.goalLeft.CreateShape(goalLeftLineDef);

            BodyDef goalRightDef = new BodyDef
            {
                Position = new Vec2(13.16f, 5.035f)
            };
            this.goalRight = this.world.CreateBody(goalRightDef);
            CircleDef goalRightBarDef = new CircleDef
            {
                Radius = 0.075f,
                LocalPosition = new Vec2(-0.425f, 0),
                Friction = 1,
                Restitution = 0,
                UserData = "Post"
            };
            PolygonDef goalRightLineDef = new PolygonDef
            {
                Friction = 1,
                Restitution = 0
            };
            goalRightLineDef.SetAsBox(0.46f, 0.001f, new Vec2(0.04f, -0.069f), 0);
            this.goalRight.CreateShape(goalRightBarDef);
            this.goalRight.CreateShape(goalRightLineDef);

            BodyDef legLeftDef = new BodyDef
            {
                Position = new Vec2(playerLeftDef.Position.X, playerLeftDef.Position.Y + 0.2f)
            };
            this.legLeft = this.world.CreateBody(legLeftDef);
            PolygonDef legLeftShapeDef = new PolygonDef
            {
                Density = 1.4f,
                Friction = 3,
                Restitution = 0,
                Vertices = new Vec2[] { new Vec2(0.12f, 0.03f), new Vec2(-0.07f, -0.06f), new Vec2(-0.03f, -0.15f) },
                VertexCount = 3
            };
            this.legLeft.CreateShape(legLeftShapeDef);
            this.legLeft.SetMassFromShapes();

            RevoluteJointDef jointLeftDef = new RevoluteJointDef
            {
                Body1 = this.playerLeft,
                Body2 = this.legLeft,
                LocalAnchor1 = new Vec2(0.05f, -0.4f),
                LocalAnchor2 = new Vec2(0, -0.64f),
                LowerAngle = AngleConversion.DegreeToRadian(-18),
                UpperAngle = AngleConversion.DegreeToRadian(13),
                EnableMotor = true,
                EnableLimit = true,
                MaxMotorTorque = 0.08f
            };
            this.jointLeft = this.world.CreateJoint(jointLeftDef);

            BodyDef legRightDef = new BodyDef
            {
                Position = new Vec2(playerRightDef.Position.X, playerRightDef.Position.Y + 0.2f)
            };
            this.legRight = this.world.CreateBody(legRightDef);
            PolygonDef legRightShapeDef = new PolygonDef
            {
                Density = 1.4f,
                Friction = 3,
                Restitution = 0,
                Vertices = new Vec2[] { new Vec2(-0.12f, 0.03f), new Vec2(0.03f, -0.15f), new Vec2(0.07f, -0.06f) },
                VertexCount = 3
            };
            this.legRight.CreateShape(legRightShapeDef);
            this.legRight.SetMassFromShapes();

            RevoluteJointDef jointRightDef = new RevoluteJointDef
            {
                Body1 = this.playerRight,
                Body2 = this.legRight,
                LocalAnchor1 = new Vec2(-0.05f, -0.4f),
                LocalAnchor2 = new Vec2(0, -0.64f),
                LowerAngle = AngleConversion.DegreeToRadian(-13),
                UpperAngle = AngleConversion.DegreeToRadian(18),
                EnableMotor = true,
                EnableLimit = true,
                MaxMotorTorque = 0.08f
            };
            this.jointRight = this.world.CreateJoint(jointRightDef);

            this.playerLeftTimers.Defend = 12000;
            this.playerRightTimers.Defend = 12000;

            this.listener.OnResult += this.BallContact;
            this.listener.OnPersist += this.BallContacting;
            this.listener.OnAdd += this.BallContacting;
            this.listener.OnAdd += this.PlayerLeftContact;
            this.listener.OnAdd += this.PlayerRightContact;
            this.listener.OnAdd += this.PlayersTackle;
            this.listener.OnRemove += this.PlayerLeftContactRemove;
            this.listener.OnRemove += this.PlayerRightContactRemove;
            this.world.SetContactListener(this.listener);
        }

        public event Action RightConceded, LeftConceded, RightPostRaised, LeftPostRaised;
        public event Action<float> BallTackled, BallPostTackled;

        public Vec2 PlayerLeftPosition => this.playerLeft.GetPosition();
        public Vec2 LegLeftPosition => this.legLeft.GetPosition();
        public float LegLeftAngle => this.legLeft.GetAngle();
        public Vec2 PlayerRightPosition => this.playerRight.GetPosition();
        public Vec2 LegRightPosition => this.legRight.GetPosition();
        public float LegRightAngle => this.legRight.GetAngle();
        public Vec2 BallPosition => this.ball.GetPosition();
        public float BallAngle => this.ball.GetAngle();

        public void Handle(float iterationDuration, PlayersInput input)
        {
            this.BallHandle();
            this.LegsHandleBeforeStep();
            this.PlayerLeftOperate(input, iterationDuration);
            this.PlayerRightOperate(input, iterationDuration);
            this.world.Step(iterationDuration / 1000, 8, 4);
            this.LegsHandleAfterStep(iterationDuration);
            this.DetectGoal();
            this.CheckImpudentDefense(iterationDuration);
        }

        private void CheckImpudentDefense(float iterationDuration)
        {
            if (this.scored || !this.shouldAttack)
                return;

            bool playerLeftDefends, playerRightDefends;

            if (this.playerLeft.GetPosition().X < 4.24f && this.playerRight.GetPosition().X * 1 > 4.24f)
                playerLeftDefends = true;
            else
                playerLeftDefends = false;

            if (this.playerRight.GetPosition().X > 9.41f && this.playerLeft.GetPosition().X * 1 < 9.41f)
                playerRightDefends = true;
            else
                playerRightDefends = false;

            if (playerLeftDefends && this.leftPostRaiseCount <= 3)
            {
                this.playerLeftTimers.Defend += iterationDuration;
                if (this.playerLeftTimers.Defend < 0)
                {
                    Vec2 temp = this.goalLeft.GetPosition();
                    this.world.DestroyBody(this.goalLeft);
                    BodyDef goalDef = new BodyDef()
                    {
                        Position = new Vec2(temp.X, temp.Y - 0.5f)
                    };
                    this.goalLeft = this.world.CreateBody(goalDef);
                    CircleDef post = new CircleDef()
                    {
                        Radius = 0.075f,
                        LocalPosition = new Vec2(0.425f, 0),
                        Density = 1,
                        Friction = 1,
                        Restitution = 0
                    };
                    this.goalLeft.CreateShape(post);
                    PolygonDef net = new PolygonDef()
                    {
                        Density = 1,
                        Friction = 1,
                        Restitution = 0
                    };
                    net.SetAsBox(0.46f, 0.001f, new Vec2(-0.04f, -0.069f), 0);
                    this.goalLeft.CreateShape(net);
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
                    Vec2 temp = this.goalRight.GetPosition();
                    this.world.DestroyBody(this.goalRight);
                    BodyDef goalDef = new BodyDef()
                    {
                        Position = new Vec2(temp.X, temp.Y - 0.5f)
                    };
                    this.goalRight = this.world.CreateBody(goalDef);
                    CircleDef post = new CircleDef()
                    {
                        Radius = 0.075f,
                        LocalPosition = new Vec2(-0.425f, 0),
                        Density = 1,
                        Friction = 1,
                        Restitution = 0
                    };
                    this.goalRight.CreateShape(post);
                    PolygonDef net = new PolygonDef()
                    {
                        Density = 1,
                        Friction = 1,
                        Restitution = 0
                    };
                    net.SetAsBox(0.46f, 0.001f, new Vec2(0.04f, -0.069f), 0);
                    this.goalRight.CreateShape(net);
                    ++this.rightPostRaiseCount;
                    this.playerRightTimers.Defend = 12000;
                    this.RightPostRaised?.Invoke();
                }
            }
        }

        private void DetectGoal()
        {
            if (this.scored)
                return;

            if (this.ball.GetPosition().Y > this.goalLeft.GetPosition().Y)
            {
                if (this.ball.GetPosition().X + ((CircleShape)this.ball.GetShapeList()).GetRadius() < 0.85f)
                {
                    this.scored = true;
                    this.LeftConceded?.Invoke();
                }
            }

            if (this.ball.GetPosition().Y > this.goalRight.GetPosition().Y)
            {
                if (this.ball.GetPosition().X - ((CircleShape)this.ball.GetShapeList()).GetRadius() > 12.81f)
                {
                    this.scored = true;
                    this.RightConceded?.Invoke();
                }
            }
        }

        private void LegsHandleBeforeStep()
        {
            float legLeftCoef = AngleConversion.RadianToDegree(this.legLeft.GetAngle()) > -2 ? 1 : (float)System.Math.Pow(System.Math.Sin(AngleConversion.DegreeToRadian(92) + this.legLeft.GetAngle()), 15),
              legRightCoef = AngleConversion.RadianToDegree(this.legRight.GetAngle()) < 2 ? 1 : (float)System.Math.Pow(System.Math.Sin(AngleConversion.DegreeToRadian(92) - this.legRight.GetAngle()), 15);
            this.legLeft.ApplyTorque(1 * legLeftCoef);
            this.playerLeftSpeeds.BeforeStep = this.playerLeft.GetLinearVelocity().X;
            float legLeftAngleDegree = AngleConversion.RadianToDegree(this.LegLeftAngle),
                legRightAngleDegree = AngleConversion.RadianToDegree(this.LegRightAngle);
            if (legLeftAngleDegree < -9 && legLeftAngleDegree > -12)
                this.legRight.ApplyTorque(legRightCoef);
            else
                this.legRight.ApplyTorque(-legRightCoef);
            this.playerRightSpeeds.BeforeStep = this.playerRight.GetLinearVelocity().X;
        }

        private void LegsHandleAfterStep(float iterationDuration)
        {
            this.playerLeftSpeeds.AfterStep = this.playerLeft.GetLinearVelocity().X;
            this.playerRightSpeeds.AfterStep = this.playerRight.GetLinearVelocity().X;
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

            this.legLeft.ApplyForce(new Vec2(dx1 * this.legLeft.GetMass(), 0), this.legLeft.GetPosition());
            this.legRight.ApplyForce(new Vec2(dx2 * this.legRight.GetMass(), 0), this.legRight.GetPosition());
        }

        private void PlayerLeftOperate(PlayersInput input, float iterationDuration)
        {
            if (this.playerLeftTimers.Jump >= 0)
                this.playerLeftTimers.Jump -= iterationDuration;

            Vec2 velocity;
            velocity = this.playerLeft.GetLinearVelocity();
            if (input.PlayerLeftLeft && (velocity.X > -2.6))
            {
                this.playerLeft.ApplyForce(new Vec2(-11, 0), this.playerLeft.GetPosition());
            }

            if (input.PlayerLeftRight && (velocity.X < 2.6))
            {
                this.playerLeft.ApplyForce(new Vec2(11, 0), this.playerLeft.GetPosition());
            }

            if (this.PlayerLeftCanJump() && (this.playerLeftTimers.Jump < 0))
            {
                if (input.PlayerLeftJump)
                {
                    this.playerLeft.ApplyImpulse(new Vec2(0, -0.78f), this.playerLeft.GetPosition());
                    this.playerLeftTimers.Jump = 100;
                }
            }

            if (this.playerLeftLegForce < -0.1f)
                this.playerLeftLegForce *= 0.97f;

            if (input.PlayerLeftKick && this.legLeft.GetAngularVelocity() > -System.Math.PI * 9)
                this.playerLeftLegForce = -1.5f;

            if (this.playerLeftLegForce < -0.1f)
            {
                this.legLeft.ApplyTorque(this.playerLeftLegForce);
            }
        }

        private void PlayerRightOperate(PlayersInput input, float iterationDuration)
        {
            if (this.playerRightTimers.Jump >= 0)
                this.playerRightTimers.Jump -= iterationDuration;

            Vec2 velocity;
            velocity = this.playerRight.GetLinearVelocity();
            if (input.PlayerRightLeft && (velocity.X > -2.6))
            {
                this.playerRight.ApplyForce(new Vec2(-11, 0), this.playerRight.GetPosition());
            }

            if (input.PlayerRightRight && (velocity.X < 2.6))
            {
                this.playerRight.ApplyForce(new Vec2(11, 0), this.playerRight.GetPosition());
            }

            if (this.PlayerRightCanJump() && (this.playerRightTimers.Jump < 0))
            {
                if (input.PlayerRightJump)
                {
                    this.playerRight.ApplyImpulse(new Vec2(0, -0.78f), this.playerRight.GetPosition());
                    this.playerRightTimers.Jump = 100;
                }
            }

            if (this.playerRightLegForce > 0.1f)
                this.playerRightLegForce *= 0.97f;

            if (input.PlayerRightKick && this.legRight.GetAngularVelocity() < System.Math.PI * 9)
                this.playerRightLegForce = 1.5f;

            if (this.playerRightLegForce > 0.1f)
            {
                this.legRight.ApplyTorque(this.playerRightLegForce);
            }
        }

        private bool PlayerLeftCanJump()
        {
            if (this.playerLeft.GetLinearVelocity().Y < -3)
                return false;

            if (this.playerLeftContacts.WithGround)
                return true;

            if (this.playerLeftContacts.WithGoal)
            {
                if ((this.playerLeft.GetPosition().X < 1.15 || this.playerLeft.GetPosition().X > 12.51)
                            && (this.playerLeft.GetPosition().Y < 4.75))
                {
                    return true;
                }
            }

            if ((this.playerLeftContacts.WithAnotherPlayer || this.playerRightContacts.WithAnotherPlayer) && this.playerRightContacts.WithGround)
                return true;

            return false;
        }

        private bool PlayerRightCanJump()
        {
            if (this.playerRight.GetLinearVelocity().Y < -3)
                return false;

            if (this.playerRightContacts.WithGround)
                return true;

            if (this.playerRightContacts.WithGoal)
            {
                if ((this.playerRight.GetPosition().X < 1.15 || this.playerRight.GetPosition().X > 12.51)
                            && (this.playerRight.GetPosition().Y < 4.75))
                {
                    return true;
                }
            }

            if ((this.playerLeftContacts.WithAnotherPlayer || this.playerRightContacts.WithAnotherPlayer) && this.playerLeftContacts.WithGround)
                return true;

            return false;
        }

        private void BallContact(ContactResult point)
        {
            Shape shape1 = point.Shape1, shape2 = point.Shape2;
            Body body1 = point.Shape1.GetBody(), body2 = point.Shape2.GetBody();
            float ballTackleSpeed;
            Body anotherBody;

            if ((anotherBody = this.GetAnotherBodyInContact(point, this.ball)) == null)
                return;
            
            if (anotherBody == this.goalLeft)
            {
                this.ball.ApplyForce(new Vec2(0.005f, 0), this.ball.GetPosition());
                if (shape1.UserData as string == "Post" || shape2.UserData as string == "Post")
                {
                    if ((ballTackleSpeed = this.GetBallCollisionSpeed()) > 0.02f)
                        this.BallPostTackled(ballTackleSpeed);

                    return;
                }
            }

            if (anotherBody == this.goalRight)
            {
                this.ball.ApplyForce(new Vec2(-0.005f, 0), this.ball.GetPosition());
                if (shape1.UserData as string == "Post" || shape2.UserData as string == "Post")
                {
                    if ((ballTackleSpeed = this.GetBallCollisionSpeed()) > 0.02f)
                        this.BallPostTackled(ballTackleSpeed);

                    return;
                }
            }

            if (anotherBody == this.ground)
            {
                if (this.ball.GetAngularVelocity() * 0.15 * (this.ball.GetAngularVelocity() > 0 ? 1 : -1) >
                        0.85 * this.ball.GetLinearVelocity().X * (this.ball.GetLinearVelocity().X > 0 ? 1 : -1))
                {
                    this.ball.SetAngularVelocity(this.ball.GetAngularVelocity() * 0.94f);
                }
            }

            if ((ballTackleSpeed = this.GetBallCollisionSpeed()) > 0.02f)
                this.BallTackled?.Invoke(ballTackleSpeed);
        }

        private void BallContacting(ContactPoint point)
        {
            Body anotherBody;

            if ((anotherBody = this.GetAnotherBodyInContact(point, this.ball)) == null)
                return;

            if (anotherBody == this.playerLeft || anotherBody == this.playerRight)
            {
                if (anotherBody == this.playerLeft)
                {
                    this.ballContacts.WithPlayerLeft = true;
                    if (this.ballContacts.WithPlayerRight)
                        point.Friction = 0;
                }
                else
                {
                    this.ballContacts.WithPlayerRight = true;
                    if (this.ballContacts.WithPlayerLeft)
                        point.Friction = 0;
                }

                if ((string)point.Shape1.UserData == "Head" || (string)point.Shape2.UserData == "Head")
                {
                    point.Restitution *= 1.1f;
                }
                else
                {
                    point.Restitution *= 0.2f;
                }                
            }
        }

        private void PlayerLeftContact(ContactPoint point)
        {
            Body anotherBody;
            if ((anotherBody = this.GetAnotherBodyInContact(point, this.playerLeft)) == null)
                return;

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
        }

        private void PlayerLeftContactRemove(ContactPoint point)
        {
            Body anotherBody;
            if ((anotherBody = this.GetAnotherBodyInContact(point, this.playerLeft)) == null)
                return;

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

        private void PlayerRightContact(ContactPoint point)
        {
            Body anotherBody;
            if ((anotherBody = this.GetAnotherBodyInContact(point, this.playerRight)) == null)
                return;

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
        }

        private void PlayerRightContactRemove(ContactPoint point)
        {
            Body anotherBody;
            if ((anotherBody = this.GetAnotherBodyInContact(point, this.playerRight)) == null)
                return;

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

        private void PlayersTackle(ContactPoint point)
        {
            if ((this.playerLeft == point.Shape1.GetBody() && this.playerRight == point.Shape2.GetBody()) ||
                (this.playerLeft == point.Shape2.GetBody() && this.playerRight == point.Shape1.GetBody()))
            {
                if (System.Math.Abs(point.Shape2.GetBody().GetPosition().Y - point.Shape2.GetBody().GetPosition().Y) < 0.7f)
                    point.Friction = 0;
                else
                    point.Friction = 5;
            }
        }

        private Body GetAnotherBodyInContact(ContactPoint point, Body body)
        {
            Body body1 = point.Shape1.GetBody(), body2 = point.Shape2.GetBody();

            if (body1 != body && body2 != body)
                return null;

            if (body1 == body)
                return body2;
            else
                return body1;
        }

        private Body GetAnotherBodyInContact(ContactResult point, Body body)
        {
            Body body1 = point.Shape1.GetBody(), body2 = point.Shape2.GetBody();

            if (body1 != body && body2 != body)
                return null;

            if (body1 == body)
                return body2;
            else
                return body1;
        }

        private float GetBallCollisionSpeed()
        {
            double alpha, beta, gamma, omega;
            alpha = System.Math.Atan2(this.ballPreviousSpeed.X, this.ballPreviousSpeed.Y);
            beta = System.Math.Atan2(-this.ball.GetLinearVelocity().X, -this.ball.GetLinearVelocity().Y);
            if (System.Math.Abs(alpha - beta) > AngleConversion.DegreeToRadian(165))
                beta = -beta;

            gamma = (alpha + beta) / 2;
            omega = System.Math.Abs(gamma - beta);
            if (omega >= AngleConversion.DegreeToRadian(85))
                return 0;

            float previousFullSpeed = (float)System.Math.Sqrt(this.ballPreviousSpeed.X * this.ballPreviousSpeed.X + this.ballPreviousSpeed.Y * this.ballPreviousSpeed.Y);
            return (float)System.Math.Cos(omega) * previousFullSpeed;
        }

        private void BallHandle()
        {
            float sinus, cosinus, x, y;
            x = this.ball.GetLinearVelocity().X;
            y = this.ball.GetLinearVelocity().Y;

            float Abs(float n) => n >= 0 ? n : -n;

            if ((Abs(x) > 0.001f) || (Abs(y) > 0.001f))
            {
                sinus = y / (float)System.Math.Sqrt(y * y + x * x);
                cosinus = x / (float)System.Math.Sqrt(y * y + x * x);
                this.ball.ApplyImpulse(new Vec2(this.ball.GetAngularVelocity() * -sinus * 0.0000006f, this.ball.GetAngularVelocity() * cosinus * 0.0000006f), this.ball.GetPosition());
            }

            this.ballPreviousSpeed = this.ball.GetLinearVelocity();
        }

        private struct PlayerContacts
        {
            public bool WithGround { get; set; }
            public bool WithAnotherPlayer { get; set; }
            public bool WithGoal { get; set; }
        }

        private struct PlayerTimers
        {
            public float Jump { get; set; }
            public float Kick { get; set; }
            public float Defend { get; set; }
        }

        private struct PlayerSpeeds
        {
            public float BeforeStep { get; set; }
            public float AfterStep { get; set; }
        }

        private struct BallContacts
        {
            public bool WithPlayerLeft { get; set; }
            public bool WithPlayerRight { get; set; }
        }
    }
}
