using System;
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
            1, 0, 0, 0, 0, 0, 0, 1,
            1, 0, 0, 0, 0, 0, 0, 1,
            1, 0, 0, 0, 0, 0, 0, 1,
            1, 0, 0, 0, 0, 0, 0, 1,
            1, 0, 0, 0, 0, 0, 0, 1,
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
                LookDir = new Vector2(0, 1),
                Right = new Vector2(1, 0),
                LookAngle = (float) Math.PI / 2
            };
            _xsize = 8;
            _ysize = 8;
            CursorVisible = false;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            Clear();
            RenderGameBoard(_gameBoard);
            RenderPlayer(_player);
            base.OnRenderFrame(e);

        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            var input = Keyboard.GetState();
            const float playerSpeed = 100f;
            const float sensitivity = 0.01f;
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
                    _player.LookAngle -= deltaX * sensitivity;

                    if (_player.LookAngle > (float) Math.PI * 2) { _player.LookAngle -= (float) Math.PI * 2; }
                    if (_player.LookAngle < 0) { _player.LookAngle += (float) Math.PI * 2; }
                    
                    _player.LookDir = new Vector2((float) Math.Cos(_player.LookAngle), (float) Math.Sin(_player.LookAngle));
                    _player.Right = new Vector2((float) Math.Sin(_player.LookAngle), (float) -Math.Cos(_player.LookAngle));
                }

                Mouse.SetPosition(1920 / 2, 1080 / 2);

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
            drawLine(player.Pos.X, -(player.Pos.Y - Height),player.Pos.X + (player.LookDir.X * 25), -(player.Pos.Y - Height) - (player.LookDir.Y * 25), new Color4(1f, 0f,0f,1f));
        }
    }
}