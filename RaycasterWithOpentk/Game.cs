using System;
using System.Collections.Generic;
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
            8,8,8,8,8,8,8,8,8,8,8,4,4,6,4,4,6,4,6,4,4,4,6,4,
            8,0,0,0,0,0,0,0,0,0,8,4,0,0,0,0,0,0,0,0,0,0,0,4,
            8,0,3,3,0,0,0,0,0,8,8,4,0,0,0,0,0,0,0,0,0,0,0,6,
            8,0,0,3,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,6,
            8,0,3,3,0,0,0,0,0,8,8,4,0,0,0,0,0,0,0,0,0,0,0,4,
            8,0,0,0,0,0,0,0,0,0,8,4,0,0,0,0,0,6,6,6,0,6,4,6,
            8,8,8,8,0,8,8,8,8,8,8,4,4,4,4,4,4,6,0,0,0,0,0,6,
            7,7,7,7,0,7,7,7,7,0,8,0,8,0,8,0,8,4,0,4,0,6,0,6,
            7,7,0,0,0,0,0,0,7,8,0,8,0,8,0,8,8,6,0,0,0,0,0,6,
            7,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,8,6,0,0,0,0,0,4,
            7,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,8,6,0,6,0,6,0,6,
            7,7,0,0,0,0,0,0,7,8,0,8,0,8,0,8,8,6,4,6,0,6,6,6,
            7,7,7,7,0,7,7,7,7,8,8,4,0,6,8,4,8,3,3,3,0,3,3,3,
            2,2,2,2,0,2,2,2,2,4,6,4,0,0,6,0,6,3,0,0,0,0,0,3,
            2,2,0,0,0,0,0,2,2,4,0,0,0,0,0,0,4,3,0,0,0,0,0,3,
            2,0,0,0,0,0,0,0,2,4,0,0,0,0,0,0,4,3,0,0,0,0,0,3,
            1,0,0,0,0,0,0,0,1,4,4,4,4,4,6,0,6,3,3,0,0,0,3,3,
            2,0,0,0,0,0,0,0,2,2,2,1,2,2,2,6,6,0,0,5,0,5,0,5,
            2,2,0,0,0,0,0,2,2,2,0,0,0,2,2,0,5,0,5,0,0,0,5,5,
            2,0,0,0,0,0,0,0,2,0,0,0,0,0,2,5,0,5,0,5,0,5,0,5,
            1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,5,
            2,0,0,0,0,0,0,0,2,0,0,0,0,0,2,5,0,5,0,5,0,5,0,5,
            2,2,0,0,0,0,0,2,2,2,0,0,0,2,2,0,5,0,5,0,0,0,5,5,
            2,2,2,2,1,2,2,2,2,2,2,1,2,2,2,5,5,5,5,5,5,5,5,5
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
        private float _bobAngle;
        private readonly Texture[] _texture = new Texture[8];

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
            _xsize = 24;
            _ysize = 24;
            _player = new Player
            {
                Pos = new Vector2((10 * (Width / _xsize)) + ((Width / _xsize) / 2), (10 * (Height / _ysize)) + ((Height / _ysize) / 2)),
                LookDir = new Vector2(0, -1),
                Right = new Vector2(-1, 0),
                LookAngle =  3f * (float)Math.PI / 2,
            };
            CursorVisible = false;
            _texture[0] = new Texture("pics/eagle.png");
            _texture[1] = new Texture("pics/redbrick.png");
            _texture[2] = new Texture("pics/purplestone.png");
            _texture[3] = new Texture("pics/greystone.png");
            _texture[4] = new Texture("pics/bluestone.png");
            _texture[5] = new Texture("pics/mossy.png");
            _texture[6] = new Texture("pics/wood.png");
            _texture[7] = new Texture("pics/colorstone.png");
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            Clear();
            
            RenderFloor(_player);
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
            foreach (var tex in _texture)
            {
                GL.DeleteTexture(tex.Handle);
            }

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
        
        private void RayCast(Player player)
        {
            int r;
            float distT = 0f;
            Color4 col = new Color4(0f, 0f, 0f,1f);

            int numRays = Width;
            List<float> angles = new List<float>();
            
            for( var x = 0; x <= Width; x++ ){
                var xAng = (float)Math.Atan( ( x - Width / 2 ) / 500f );
                xAng += player.LookAngle;
                if (xAng > Math.PI * 2) { xAng -= (float) Math.PI * 2; }
                if (xAng < 0) { xAng += (float) Math.PI * 2; }
                angles.Add(xAng);
            }

            var bobOffset = (float) Math.Sin(_bobAngle) * 10f;
            for (r = 0; r < numRays; r++)
            {
                var ra = angles[r];

                int textureHandle=1;
                
                var tx = CastRay(player, ra, ref distT, ref col, ref textureHandle);

                float ca = player.LookAngle - ra; if (ca > Math.PI * 2) { ca -= (float)Math.PI * 2; } if (ca < 0) { ca += (float)Math.PI * 2; }
                
                
                distT = distT * (float)Math.Cos(ca);
                float lineH = ((Height / _ysize) * Height) / distT;
                float lineO = (Height - lineH) / 2;
                drawTexturedLine(r, lineO + bobOffset,tx,0, r, lineH + lineO + bobOffset,tx,1, _texture[textureHandle - 1], col);
            }
        }

       


        [SuppressMessage("ReSharper", "RedundantAssignment")]
        private float CastRay(Player player, float ra, ref float distT, ref Color4 col, ref int texture)
        {
            int mx, my, mp, t1=0, t2=0;
            float rx, ry, xo, yo;
            var dof = 0;
        
            distT = 0;
            float distH = 1000000f, hx = player.Pos.X;
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
            while (dof < _ysize)
            {
                mx = (int) rx / (Width / _xsize);
                my = (int) ry / (Height / _ysize);
                mp = my * _xsize + mx;
                if (mp < _gameBoard.Length - 1 && mp > 0)
                {
                    if (_gameBoard[mp] != 0 || _gameBoard[mp - _xsize] != 0)
                    {
                        var temp = new Vector2(rx - player.Pos.X, ry - player.Pos.Y);
                        distH = temp.Length;
                        hx = rx;
                        t1 = _gameBoard[mp];
                        if (t1 == 0) { t1 = _gameBoard[mp - _xsize];}
                        
                        break;
                    }
                }

                rx += xo;
                ry += yo;
                dof += 1;

            }
            
            dof = 0;
            float distV = 1000000f, vy = player.Pos.Y;
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
            while (dof < _xsize)
            {
                mx = (int) rx / (Width / _xsize);
                my = (int) ry / (Height / _ysize);
                mp = my * _xsize + mx;
                if (mp < _gameBoard.Length - 1 && mp > 0)
                {
                    if (_gameBoard[mp] != 0 || _gameBoard[mp - 1] != 0)
                    {
                        var temp = new Vector2(rx - player.Pos.X, ry - player.Pos.Y);
                        distV = temp.Length;
                        vy = ry;
                        t2 = _gameBoard[mp];
                        if (t2 == 0) { t2 = _gameBoard[mp - 1];}
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
                ry = vy;
                distT = distV;
                col = new Color4(1f, 1f, 1f, 1f);
                tx = (ry / (Height / _ysize)) - (float) Math.Floor(ry / (Height / _ysize));
                texture = t2;
            }
            else
            {
                rx = hx;
                distT = distH;
                col = new Color4(0.5f, 0.5f, 0.5f, 1f);
                tx = (rx / (Width / _xsize)) - (float) Math.Floor(rx / (Width / _xsize));
                texture = t1;
            }
            
            return tx; 
        }

        private void RenderFloor(Player player)
        {
            float scale = ((Width / 1000f) + (Height / 1000f)) / 2;
            float planeX = player.Right.X * scale, planeY = player.Right.Y * scale; //the 2d raycaster version of camera plane
            float dirX = player.LookDir.X, dirY = player.LookDir.Y; //initial direction vector

            for(int y = Height / 2; y < Height + 10f; y++)
            {
              // rayDir for leftmost ray (x = 0) and rightmost ray (x = w)
              float rayDirX0 = dirX + planeX;
              float rayDirY0 = dirY + planeY;
              float rayDirX1 = dirX - planeX;
              float rayDirY1 = dirY - planeY;

              // Current y position compared to the center of the screen (the horizon)
              int p = y - (Height / 2);

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

              Texture ceiling = _texture[6];
              Texture floor = _texture[3];
              
              var bobOffset = (float) Math.Sin(_bobAngle) * 10f;

              
              drawTexturedLine(0, y + bobOffset, tx1, ty1, Width, y + bobOffset, tx2, ty2, floor, new Color4(0.4f, 0.4f, 0.4f, 1f));
              drawTexturedLine(0, Height - y - 1 + bobOffset, tx1, ty1, Width, Height - y - 1 + bobOffset, tx2, ty2, ceiling, new Color4(0.4f, 0.4f, 0.4f, 1f));
   
              
            }
        }
    }
}