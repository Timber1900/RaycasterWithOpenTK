using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Mime;
using System.Security.Cryptography;
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
        private float _planeX;
        private float _planeY;
        
        
        private readonly int[] _gameBoard =
        {
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1,
            1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1,
            1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1,
            1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1,
            1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1,
            1, 0, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 0, 1,
            1, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 0, 1,
            1, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 0, 1,
            1, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 0, 1,
            1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1,
            1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1,
            1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1,
            1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1,
            1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,

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
        private float _bobAngle = 0.0f;
        private Texture _floor = new Texture("Resources/greystone.png");
        
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
            _xsize = 16;
            _ysize = 16;
            CursorVisible = false;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            Clear();
            
            RenderFloor(_player, _floor);
            RayCast(_player);
            base.OnRenderFrame(e);

        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            var input = Keyboard.GetState();
            const float playerSpeed = 250f;
            const float sensitivity = 0.001f;
            if (Focused)
            {
                if (input.IsKeyDown(Key.W))
                {
                    var tempPos = _player.Pos + _player.LookDir * playerSpeed * (float) e.Time; // Forward
                    UpdatePosWithCollisions(tempPos, _player.LookAngle);
                    _bobAngle += 0.2f;
                    if (_bobAngle > Math.PI * 2) { _bobAngle -= (float) Math.PI * 2; }
                    if (_bobAngle < 0 ){ _bobAngle += (float) Math.PI * 2; }
                        
                }

                if (input.IsKeyDown(Key.S))
                {
                    var tempPos = _player.Pos - _player.LookDir * playerSpeed * (float) e.Time; // Backwards
                    float pa = _player.LookAngle - (float)Math.PI; if (pa < 0) { pa += (float) Math.PI * 2; } if (pa > Math.PI * 2) { pa -= (float) Math.PI * 2; }
                    UpdatePosWithCollisions(tempPos, pa);
                    _bobAngle += 0.2f;
                    if (_bobAngle > Math.PI * 2) { _bobAngle -= (float) Math.PI * 2; }
                    if (_bobAngle < 0 ){ _bobAngle += (float) Math.PI * 2; }
                }

                if (input.IsKeyDown(Key.A))
                {
                    var tempPos = _player.Pos + _player.Right * (playerSpeed / 2) * (float) e.Time; // Left
                    float pa = _player.LookAngle  - ((float) Math.PI / 2); if (pa < 0) { pa += (float) Math.PI * 2; } if (pa > Math.PI * 2) { pa -= (float) Math.PI * 2; }
                    UpdatePosWithCollisions(tempPos, pa);
                }

                if (input.IsKeyDown(Key.D))
                {
                    var tempPos = _player.Pos - _player.Right * (playerSpeed / 2) * (float) e.Time; // Left
                    float pa = _player.LookAngle + ((float) Math.PI / 2); if (pa < 0) { pa += (float) Math.PI * 2; } if (pa > Math.PI * 2) { pa -= (float) Math.PI * 2; }
                    UpdatePosWithCollisions(tempPos, pa);
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

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
            GL.DeleteTexture(_floor.Handle);

        }

        private void UpdatePosWithCollisions(Vector2 tempPos, float pa)
        {
            float paddingx, paddingy;
            if (pa > 0 && pa <= Math.PI / 2)
            {
                paddingx = 0.2f;
                paddingy = 0.2f;
            }
            else if (pa > Math.PI / 2 && pa <= Math.PI)
            {
                paddingx = -0.2f;
                paddingy = 0.2f; 
            }
            else if (pa > Math.PI && pa <= (3 * Math.PI) / 2)
            {
                paddingx = -0.2f;
                paddingy = -0.2f; 
            }
            else
            {
                paddingx = 0.2f;
                paddingy = -0.2f; 
            }
            var mx = (int)Math.Floor((_player.Pos.X / (Width / _xsize)) + paddingx);
            var my = (int)Math.Floor((tempPos.Y / (Height / _ysize)) + paddingy);
                    
            var mp = my * _xsize + mx;
                
            if (_gameBoard[mp] == 0)
            {
                _player.Pos.Y = tempPos.Y;
            }

            mx = (int)Math.Floor((tempPos.X / (Width / _xsize)) + paddingx);
            my = (int)Math.Floor((_player.Pos.Y / (Height / _ysize)) + paddingy);
            mp = my * _xsize + mx;
            if (_gameBoard[mp] == 0)
            {
                _player.Pos.X = tempPos.X;
            }
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
        
        private void RayCast(Player player)
        {
            int r;
            float ra, distT = 0f;
            Color4 col = new Color4(0f, 0f, 0f,1f);
            Texture texture = new Texture("Resources/wall.png");

            int numRays = Width;
            List<float> angles = new List<float>();
            
            for( var x = 0; x <= Width; x++ ){
                var xAng = (float)Math.Atan( ( x - Width / 2 ) / 500f );
                xAng += player.LookAngle;
                if (xAng > Math.PI * 2) { xAng -= (float) Math.PI * 2; }
                if (xAng < 0) { xAng += (float) Math.PI * 2; }
                angles.Add(xAng);
            }

            float fov = angles[^1] - angles[0];
            if (fov > Math.PI * 2) { fov -= (float) Math.PI * 2; }
            if (fov < 0) { fov += (float) Math.PI * 2; }

            float size = (float) Math.Tan(fov / 2) * 2;
            
            player.Right.Normalize();
            player.Right *= size;
            _planeX = player.Right.X;
            _planeY = player.Right.Y;
            player.Right.Normalize();

            float bobOffset = (float) Math.Sin(_bobAngle) * 10f;
            for (r = 0; r < numRays; r++)
            {
                ra = angles[r];
                
                var tx = CastRay(player, ra, ref distT, ref col);

                float ca = player.LookAngle - ra; if (ca > Math.PI * 2) { ca -= (float)Math.PI * 2; } if (ca < 0) { ca += (float)Math.PI * 2; }
                
                
                
                distT = distT * (float)Math.Cos(ca);
                float lineH = ((Height / _ysize) * Height) / distT;
                float lineO = (Height - lineH) / 2;
                drawTexturedLine(r, lineO,tx,0, r, lineH + lineO,tx,1, texture, col);
            }
            
            GL.DeleteTexture(texture.Handle);
            
        }
        private float CastRay(Player player, float ra, ref float distT, ref Color4 col)
        {
            int mx, my, mp;
            float rx, ry, xo, yo;
            var dof = 0;
        
            distT = 0;
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
            while (dof < 16)
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
            while (dof < 16)
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

            float tx;
            if(distV<=distH)
            {
                rx = vx;
                ry = vy;
                distT = distV;
                col = new Color4(1f, 1f, 1f, 1f);
                tx = (ry / (Height / _ysize)) - (float) Math.Floor(ry / (Height / _ysize));

            }
            else
            {
                rx = hx;
                ry = hy;
                distT = distH;
                col = new Color4(0.5f, 0.5f, 0.5f, 1f);
                tx = (rx / (Width / _xsize)) - (float) Math.Floor(rx / (Width / _xsize));
            }
            
            return tx; 
            //drawLine(player.Pos.X, player.Pos.Y, rx, ry, new Color4(0f, 0.3f, 0.7f, 0.5f));
        }

        private void RenderFloor(Player player, Texture texture)
        {
            float planeX = player.Right.X, planeY = player.Right.Y; //the 2d raycaster version of camera plane
            float dirX = player.LookDir.X, dirY = player.LookDir.Y; //initial direction vector

            for(int y = Height/2; y < Height; y++)
            {
              // rayDir for leftmost ray (x = 0) and rightmost ray (x = w)
              float rayDirX0 = dirX + planeX;
              float rayDirY0 = dirY + planeY;
              float rayDirX1 = dirX - planeX;
              float rayDirY1 = dirY - planeY;

              // Current y position compared to the center of the screen (the horizon)
              int p = y - Height / 2;

              // Vertical position of the camera.
              float posZ = 0.5f * Height;

              // Horizontal distance from the camera to the floor for the current row.
              // 0.5 is the z position exactly in the middle between floor and ceiling.
              float rowDistance = posZ / p;

              // calculate the real world step vector we have to add for each x (parallel to camera plane)
              // adding step by step avoids multiplications with a weight in the inner loop
              float floorStepX = rowDistance * (rayDirX1 - rayDirX0) / Width;
              float floorStepY = rowDistance * (rayDirY1 - rayDirY0) / Height;

              // real world coordinates of the leftmost column. This will be updated as we step to the right.
              float floorX = (player.Pos.X / (Width / _xsize)) + rowDistance * rayDirX0;
              float floorY = (player.Pos.Y / (Height / _ysize)) + rowDistance * rayDirY0;

              
              int cellX = (int)(floorX);
              int cellY = (int)(floorY);

              // get the texture coordinate from the fractional part
              float tx1 = ((int) (64 * (floorX - cellX))) / 64f;
              float ty1 = ((int) (64 * (floorY - cellY))) / 64f;
              
              float tx2 = ((int) (64 * ((floorX + (floorStepX * Width)) - cellX))) / 64f;
              float ty2 = ((int) (64 * ((floorY + (floorStepY * Height)) - cellY))) / 64f;
              
              drawTexturedLine(0, y, tx1, ty1, Width, y, tx2, ty2, texture, new Color4(1f, 1f, 1f, 1f));
              
              
            }
        }
    }
}