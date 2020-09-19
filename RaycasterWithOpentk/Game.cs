using System;
using System.Diagnostics.CodeAnalysis;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.ES11;
using OpenTK.Input;
using Program;
using Boolean = System.Boolean;

namespace RaycasterWithOpentk
{
    public class Game : MainRenderWindow
    {
        private int _xsize, _ysize;
        
        private readonly int[] _gameBoard =
        {
            1, 1, 1, 1, 1, 1, 1, 1,
            1, 0, 1, 0, 0, 0, 0, 1,
            1, 0, 1, 0, 0, 0, 0, 1,
            1, 0, 1, 0, 0, 0, 0, 1,
            1, 0, 0, 0, 0, 0, 0, 1,
            1, 0, 0, 0, 0, 1, 0, 1,
            1, 0, 0, 0, 0, 0, 0, 1,
            1, 1, 1, 1, 1, 1, 1, 1
        };
        private struct Player
        {
            public Vector2 Pos;
            public Vector2 LookDir;
            public Vector2 Right;
            public float LookAngle;

        };

        private Player _player;
        private Boolean _firstMove = true;
        private Vector2 _lastPos;
        
        public Game(int width, int height, string title)
            : base(width, height, title)
        {
        }

        protected override void OnLoad(EventArgs e)
        {
            setClearColor(new Color4(0.0f, 0.0f, 0.0f, 1.0f)); //Sets Background Color
            UseDepthTest = false; //Enables Depth Testing for 3D
            RenderLight = false; //Makes the 3D light visible
            UseAlpha = true; //Enables alpha use
            KeyboardAndMouseInput = false; //Enables keyboard and mouse input for 3D movement
            base.OnLoad(e);
            _player = new Player
            {
                Pos = new Vector2(500, 500),
                LookDir = new Vector2(0, -1),
                Right = new Vector2(-1, 0),
                LookAngle =  3f * (float)Math.PI / 2
            };
            _xsize = 8;
            _ysize = 8;
            CursorVisible = false;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            Clear();
            RenderGameBoard(_gameBoard);
            RayCast(_player, (float) Math.PI / 2);
            RenderPlayer(_player);

            base.OnRenderFrame(e);

        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            var input = Keyboard.GetState();
            const float playerSpeed = 100f;
            const float sensitivity = 0.001f;
            if (Focused)
            {
                if (input.IsKeyDown(Key.W))
                {
                    _player.Pos += _player.LookDir * playerSpeed * (float) e.Time; // Forward
                }

                if (input.IsKeyDown(Key.S))
                {
                    _player.Pos -= _player.LookDir * playerSpeed * (float) e.Time; // Forward
                }

                if (input.IsKeyDown(Key.A))
                {
                    _player.Pos -= _player.Right * playerSpeed * (float) e.Time; // Left
                }

                if (input.IsKeyDown(Key.D))
                {
                    _player.Pos += _player.Right * playerSpeed * (float) e.Time; // Left
                }

                // Get the mouse state
                var mouse = Mouse.GetState();

                if (_firstMove) // this bool variable is initially set to true
                {
                    _lastPos = new Vector2(mouse.X, mouse.Y);
                    _firstMove = false;
                }
                else
                {
                    // Calculate the offset of the mouse position
                    var deltaX = mouse.X - _lastPos.X;
                    _lastPos = new Vector2(mouse.X, mouse.Y);

                    // Apply the camera pitch and yaw (we clamp the pitch in the camera class)
                    _player.LookAngle += deltaX * sensitivity;

                    if (_player.LookAngle > (float) Math.PI * 2) { _player.LookAngle -= (float) Math.PI * 2; }
                    if (_player.LookAngle < 0) { _player.LookAngle += (float) Math.PI * 2; }
                
                    _player.LookDir = new Vector2((float) Math.Cos(_player.LookAngle), (float) Math.Sin(_player.LookAngle));
                    _player.Right = new Vector2((float) Math.Sin(_player.LookAngle), (float) -Math.Cos(_player.LookAngle));
                }

                Mouse.SetPosition(1920 / 2, 1080 / 2);
            }
            
            
            
            base.OnUpdateFrame(e);
        }

        private void RenderGameBoard(int[] gameBoard)
        {
            var xoff = Width / _xsize;
            var yoff = Height / _ysize;
            
            for (var y = 0; y < _ysize; y++)
            {
                for (var x = 0; x < _xsize; x++)
                {
                    var state = gameBoard[y * _ysize + x];
                    Color4 col = new Color4(0.2f, 0.2f, 0.2f, 1.0f);
                    switch (state)
                    {
                        case 1:
                            col = new Color4(1.0f, 1.0f, 1.0f, 1.0f);
                            break;
                    }
                    
                    drawRectangle(x * xoff + 1, y * yoff + 1, (x + 1) * xoff - 1, (y + 1) * yoff - 1, col);
                }
            }
        }

        private void RenderPlayer(Player player)
        {
            drawEllipse(player.Pos.X, player.Pos.Y, 10,10, new Color4(0f,0f,1f,1f));
            drawLine(player.Pos.X, player.Pos.Y ,player.Pos.X + (player.LookDir.X * 25), player.Pos.Y + (player.LookDir.Y * 25), new Color4(1f, 0f,0f,1f));
        }

        
        private void RayCast(Player player, float fov)
        {
            int r, mx, my, mp, dof;
            float rx, ry, ra, xo, yo;

            ra = player.LookAngle - fov / 2;
            if (ra > Math.PI * 2) { ra -= (float)Math.PI * 2; }
            if (ra < 0) { ra += (float)Math.PI * 2; }
            int numRays = 1000;
            float step = fov / numRays;
            for (r = 0; r < numRays; r++)
            {
                dof = 0;
                float distH = 1000000f, hx = player.Pos.X, hy = player.Pos.Y;
                float aTan = -1 / (float) Math.Tan(ra);
                if (ra > Math.PI)
                {
                    ry = (float)Math.Floor(player.Pos.Y / (Height / _ysize) - 0.01) * (Height / _ysize);
                    rx = Math.Abs(player.Pos.Y - ry) * aTan + player.Pos.X;
                    yo = -(Height / _ysize);
                    xo = -yo * aTan;
                }
                else if (ra < Math.PI)
                {
                    ry = (float)Math.Floor(player.Pos.Y / (Height / _ysize) + 1.01) * (Height / _ysize);
                    rx = -Math.Abs(player.Pos.Y - ry) * aTan + player.Pos.X;
                    yo = (Height / _ysize);
                    xo = -yo * aTan;
                }
                else
                {
                    rx = player.Pos.X;
                    ry = player.Pos.Y;
                    yo = 0;
                    xo = 0;
                    dof = 8;
                }
                while (dof < 8)
                {
                    mx = (int) rx / (Width / _xsize);
                    my = (int) ry / (Height / _ysize);
                    mp = my * _xsize + mx;
                    if (mp < _gameBoard.Length - 1 && mp > 0)
                    {
                        if (_gameBoard[mp] == 1 || _gameBoard[mp - _xsize] == 1)
                        {
                            var temp = new Vector2(rx - player.Pos.X, ry - player.Pos.Y);
                            distH = temp.Length;
                            hx = rx;
                            hy = ry;
                            break;
                        }
                    }

                    rx += xo;
                    ry += yo;
                    dof += 1;
   
                }
                
                dof = 0;
                float distV = 1000000f, vx = player.Pos.X, vy = player.Pos.Y;
                float nTan = -(float)Math.Tan(ra);
                if (ra > Math.PI / 2 && ra < (Math.PI * 3) / 2)
                {
                    rx = (float)Math.Floor(player.Pos.X / (Width / _xsize) - 0.01) * (Width / _xsize);
                    ry = Math.Abs(player.Pos.X - rx) * nTan + player.Pos.Y;
                    xo = -(Width / _xsize);
                    yo = -xo * nTan;
                }
                else if (ra < Math.PI / 2 || ra > (Math.PI * 3) / 2)
                {
                    rx = (float)Math.Floor(player.Pos.X / (Width / _xsize) + 1.01) * (Width / _xsize);
                    ry = -Math.Abs(player.Pos.X - rx) * nTan + player.Pos.Y;
                    xo = (Width / _xsize);
                    yo = -xo * nTan;
                }
                else
                {
                    rx = player.Pos.X;
                    ry = player.Pos.Y;
                    yo = 0;
                    xo = 0;
                    dof = 8;
                }
                while (dof < 8)
                {
                    mx = (int) rx / (Width / _xsize);
                    my = (int) ry / (Height / _ysize);
                    mp = my * _xsize + mx;
                    if (mp < _gameBoard.Length - 1 && mp > 0)
                    {
                        if (_gameBoard[mp] == 1 || _gameBoard[mp - 1] == 1)
                        {
                            var temp = new Vector2(rx - player.Pos.X, ry - player.Pos.Y);
                            distV = temp.Length;
                            vx = rx;
                            vy = ry;
                            break;
                        }
                        
                    }
                    
                    rx += xo;
                    ry += yo;
                    dof += 1;
   
                }

                
                if(distV<=distH)
                {
                    rx = vx;
                    ry = vy;
                }
                if(distH<distV)
                {
                    rx = hx;
                    ry = hy;
                }
                drawLine(player.Pos.X, player.Pos.Y, rx, ry, new Color4(0f, 0.3f, 0.7f, 0.5f));  
                //drawEllipse(rx, ry, 5, 5, new Color4(1f, 0f, 0f, 0.5f));
                

                ra += step;

                if (ra > Math.PI * 2) { ra -= (float)Math.PI * 2; }
                if (ra < 0) { ra += (float)Math.PI * 2; }
            }
            
        }
    }
}