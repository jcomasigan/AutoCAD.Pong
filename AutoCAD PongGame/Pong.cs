using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PongGame
{
    class Pong
    {
        public Pong()
        {

        }

        Entity userPaddle, enemyPaddle, ball, bbox, scoreLabelEnt;
        public static bool UpKey = false;
        double ballY = 0.03;
        double ballX = 0;
        double ballSpeed = 0.00002;
        double enemyY = 0.02;
        int curScore = 0;
        //Random ballDirection = new Random();
        Random randomNum = new Random();
        public static string keyPress = ""; //KeyPresHandler form sends key input in to this var
        KeyPressHandler keyHandler = new KeyPressHandler();
        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        Document docPong = Application.DocumentManager.MdiActiveDocument;

        /// <summary>
        /// Starts The Pong Game
        /// </summary>
        public void StartGame()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            if (doc != null)
            {
                ed.WriteMessage("\n USE THE UP AND DOWN ARROW KEYS TO MOVE THE PADDE. \n \n PRESS ESCAPE OR CLOSE DIALOG BOX TO STOP THE GAME.");
                PromptStringOptions promptOptions = new PromptStringOptions("\n Run Pong Game? \n Type Y or N");
                PromptResult run = ed.GetString(promptOptions);
                if (run.StringResult.ToUpper() == "Y")
                {
                    #region Create the entities
                    bbox = DrawEntity.DrawBox(new Point2d(0, 20), new Point2d(20, 20), new Point2d(20, 0), new Point2d(0, 0), "PongGameBoundingBox");
                    userPaddle = DrawEntity.DrawBox(new Point2d(2, 2), new Point2d(2.25, 2), new Point2d(2.25, 6), new Point2d(2, 6), "PongGamePaddle", 50);
                    enemyPaddle = DrawEntity.DrawBox(new Point2d(17.75, 2), new Point2d(18, 2), new Point2d(18, 6), new Point2d(17.75, 6), "PongGamePaddle", 50);
                    scoreLabelEnt = DrawEntity.DrawText(new Point2d(7, 21.5), "SCORE: " + curScore.ToString(), "PongGameScore", 30);
                    ball = DrawEntity.DrawCircle(0.35, new Point3d(10, 10, 0), "PongGameBall", 10);
                    #endregion
                    DrawEntity.ZoomExtents(); //Zoom extents.
                    timer.Interval = 10;
                    timer.Tick += GameEngine;
                    timer.Start();
                    keyHandler.Show(); //Show modeless form that listens for key inputs. 
                }
            }
        }

        private void AddSpeedToBall() //Accelerates the ball
        {
            if (Math.Sign(ballX) == -1)
            {
                ballX -= ballSpeed;
            }
            else
            {
                ballX += ballSpeed;
            }
            if (Math.Sign(ballY) == -1)
            {
                ballY -= ballSpeed;
            }
            else
            {
                ballY += ballSpeed;
            }
        }

        private void GameEngine(object sender, EventArgs e)
        {
            if(keyPress == "QUIT") //If user press escape button
            {
                if(timer.Enabled)
                {
                    timer.Stop();
                    keyPress = "";
                }
            }
            if (docPong == Application.DocumentManager.MdiActiveDocument) //Check if we are in the same document as when PongGame was instantiated
            {
                keyHandler.Focus();          
                Database db = docPong.Database;
                Editor ed = docPong.Editor;
                AddSpeedToBall();
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    BlockTable blockTable = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    Polyline paddle = tr.GetObject(userPaddle.ObjectId, OpenMode.ForRead) as Polyline;
                    Polyline enemy = tr.GetObject(enemyPaddle.ObjectId, OpenMode.ForRead) as Polyline;
                    Circle cball = tr.GetObject(ball.ObjectId, OpenMode.ForRead) as Circle;
                    Point3d originPoint = new Point3d(0, 0, 0);
                    Vector3d ballVector = originPoint.GetVectorTo(new Point3d(ballX, ballY, 0));
                    Vector3d playerVector = originPoint.GetVectorTo(new Point3d(0, 0, 0));
                    Vector3d enemyVector = originPoint.GetVectorTo(new Point3d(0, 0, 0));                    
                    bool startOver = false; //Switch to restart game after scoring
                    int numOfPlayerPaddleVert = paddle.NumberOfVertices;
                    int numOfEnemyPaddleVert = enemy.NumberOfVertices;
                    Point3d ballLocation = cball.Center;
                    double ballRadius = cball.Radius;
                    #region Collision Detection For Paddles 
                    //Detects if ball collides with the player & enemy paddle 
                    Point3dCollection collisionPoints = new Point3dCollection();
                    IntPtr ptr1 = new IntPtr();
                    IntPtr ptr2 = new IntPtr();
                    cball.BoundingBoxIntersectWith(paddle, Intersect.OnBothOperands, collisionPoints, ptr1, ptr2);
                    if (collisionPoints.Count > 0)
                    {                       
                        double oldBallX = ballX;
                        ballX = randomNum.NextDouble() * (0.05 - 0.01) + 0.05;
                        if (Math.Sign(oldBallX) == Math.Sign(ballX))
                        {
                            ballX *= -1; //reverse direction
                        }
                    }
                    collisionPoints = new Point3dCollection();
                    ptr1 = new IntPtr();
                    ptr2 = new IntPtr();
                    cball.BoundingBoxIntersectWith(enemy, Intersect.ExtendBoth, collisionPoints, ptr1, ptr2);
                    if (collisionPoints.Count > 0)
                    {
                        double oldBallX = ballX;
                        ballX = randomNum.NextDouble() * (0.05 - 0.01) + 0.05;
                        if (Math.Sign(oldBallX) == Math.Sign(ballX))
                        {
                            ballX *= -1; //reverse direction                           
                        }                       
                    }
                    #endregion
                    #region Game Area Bounds
                    //Checks where the ball is still in the game area
                    if (ballLocation.X + ballRadius >= 20 || ballLocation.X - ballRadius <= 0)
                    {
                        //Calculate Score Based on what side it hits
                        if(ballLocation.X + ballRadius >= 20)
                        {
                            //Right side gets hit. Player scores
                            curScore += 1;
                        }
                        if (ballLocation.X - ballRadius <= 0)
                        {
                            //Left side gets hit. Enemy scores
                            curScore -= 1;
                        }
                        startOver = true;
                    }
                    if (ballLocation.Y + ballRadius >= 20 || ballLocation.Y - ballRadius <= 0)
                    {
                        double oldBallY = ballY;
                        ballY = randomNum.NextDouble() * (0.05 - 0.01) + 0.05;
                        if (Math.Sign(oldBallY) == Math.Sign(ballY))
                        {                             
                            ballY *= -1;
                        }
                    }
                    #endregion
                    #region Enemy AI
                    for (int i = 0; i < numOfEnemyPaddleVert; i++)
                    {
                        Point2d point = enemy.GetPoint2dAt(i);
                        if (i > 2) //Last two vertices are the top side of the paddle
                        {
                            if (point.Y < ballLocation.Y)
                            {
                                    if (point.Y < 20)
                                    {
                                        enemyY = randomNum.NextDouble() * (0.05 - 0.005) + 0.05;
                                        enemyVector = originPoint.GetVectorTo(new Point3d(0, enemyY, 0));
                                    }
                                    else
                                    {
                                        enemyVector = originPoint.GetVectorTo(new Point3d(0, 0, 0));
                                    }                               
                            }
                        }
                        if (i < 2) //First two vertices are the bottom side of the paddle
                        {
                                if (point.Y > ballLocation.Y)
                                {
                                    if (point.Y > 0)
                                    {
                                        enemyY = randomNum.NextDouble() * (0.05 - 0.005) + 0.05;
                                        enemyY *= -1;
                                        enemyVector = originPoint.GetVectorTo(new Point3d(0, enemyY, 0));
                                    }
                                    else
                                    {
                                        enemyVector = originPoint.GetVectorTo(new Point3d(0, 0, 0));
                                    }
                                }                         
                        }
                    }
                    #endregion
                    #region Key Presses
                    bool canMove = false;
                    switch (keyPress)
                    {
                        case "UP":
                            {
                                for (int i = 2; i < numOfPlayerPaddleVert; i++)
                                {
                                    Point2d point = paddle.GetPoint2dAt(i);
                                    if (point.Y < 20 && point.Y > 0)
                                    {
                                        canMove = true;
                                    }
                                    else
                                    {
                                        canMove = false;
                                    }
                                }
                                if (canMove)
                                {
                                    playerVector = originPoint.GetVectorTo(new Point3d(0, 0.5, 0));
                                }
                                keyPress = "";
                                break;
                            }
                        case "DOWN":
                            {
                                for (int i = 0; i < numOfPlayerPaddleVert - 2; i++)
                                {
                                    Point2d point = paddle.GetPoint2dAt(i);
                                    if (point.Y < 20 && point.Y > 0)
                                    {
                                        canMove = true;
                                    }
                                    else
                                    {
                                        canMove = false;
                                    }
                                }
                                if (canMove)
                                {
                                    playerVector = originPoint.GetVectorTo(new Point3d(0, -0.5, 0));
                                }
                                keyPress = "";
                                break;
                            }
                    }
                    #endregion
                    #region Update Entities Location & Vals
                    using (DocumentLock doclock = docPong.LockDocument()) //Document needs to be locked before changing their properties
                    {
                        cball = tr.GetObject(ball.ObjectId, OpenMode.ForWrite) as Circle;
                        userPaddle = tr.GetObject(userPaddle.ObjectId, OpenMode.ForWrite) as Polyline;
                        enemyPaddle = tr.GetObject(enemyPaddle.ObjectId, OpenMode.ForWrite) as Polyline;
                        MText scoreLabelMtext = tr.GetObject(scoreLabelEnt.ObjectId, OpenMode.ForWrite) as MText;
                        scoreLabelMtext.Contents = "SCORE: " + curScore.ToString();
                        if(!startOver)
                        {
                            ballVector = originPoint.GetVectorTo(new Point3d(ballX, ballY, 0));
                            cball.TransformBy(Matrix3d.Displacement(ballVector));
                        }
                        else
                        {
                            cball.Center = new Point3d(10, 10, 0);
                            //restart ball speed & randomise direction
                            ballY = randomNum.NextDouble() * (0.03 - 0.01) + 0.03;
                            ballX = randomNum.NextDouble() * (0.03 - 0.01) + 0.03;
                            Random randSign = new Random();
                            if((randSign.Next() * (3 - 1) + 1) == 1)
                            {
                                ballY *= -1;
                            }
                            if ((randSign.Next() * (3 - 1) + 1) == 2)
                            {
                                ballX *= -1;
                            }

                            ballSpeed = 0.00002;
                            startOver = false;                            
                        }

                        paddle.TransformBy(Matrix3d.Displacement(playerVector));
                        enemyPaddle.TransformBy(Matrix3d.Displacement(enemyVector));
                    }
                    #endregion
                    ed.Regen();
                    tr.Commit();
                }
            }
            else
            {
            }
        }

    }
}
