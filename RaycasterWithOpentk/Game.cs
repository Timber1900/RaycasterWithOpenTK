using System;
using OpenTK;
using OpenTK.Graphics;
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

            _xsize = 8;
            _ysize = 8;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            Clear();
            RenderGameBoard(_gameBoard);
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
                    Color4 col = new Color4(0.0f, 0.0f, 0.0f, 1.0f);
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
    }
}