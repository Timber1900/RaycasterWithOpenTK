using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.ES11;
using Program;

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
        };

        private Player _player;
        
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
                LookDir = new Vector2(0, 1)
            };
            _xsize = 8;
            _ysize = 8;
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
            drawLine(player.Pos.X, player.Pos.Y,player.Pos.X + (player.LookDir.X * 25), player.Pos.Y - (player.LookDir.Y * 25), new Color4(1f, 0f,0f,1f));
        }
    }
}