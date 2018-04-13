namespace MoonImpact.Gui.Utilities
{
    using System;
    using SharpDX;
    using SharpDX.RawInput;

    public class Camera
    {
        //private MouseState _lastState;

        private readonly ICameraMatrices _cameraMatrices;

        private readonly RawInput _rawInput;

        public Camera(ICameraMatrices cameraMatrices)
        {
            _cameraMatrices = cameraMatrices;
            _rawInput = new RawInput();
            _rawInput.MouseInput += MouseInput;
            UpdateMatrices();
        }

        private double _yaw = 0;

        private double _pitch = 0;

        private float _scale = 1f;

        private bool _matricesNeedUpdating;

        public Matrix View { get; private set; }

        public Vector3 Target { get; set; }

        public float Distance { get; set; } = 100;

        private bool _leftMouseDown = false;

        private bool _rightMouseDown = false;

        private Vector3 _right;

        private Vector3 _up;

        private Vector3 _forward;
        
        private void MouseInput(object sender, MouseInputEventArgs mouseInputEventArgs)
        {
            if (mouseInputEventArgs.WheelDelta != 0)
            {
                const float Sensitivity = 0.001f;
                var scaleAmt = MathUtil.Clamp(1 + mouseInputEventArgs.WheelDelta * Sensitivity, 0.1f, 10f);
                _scale *= scaleAmt;
                Console.Out.WriteLine(_scale);
                _matricesNeedUpdating = true;
            }

            if (mouseInputEventArgs.ButtonFlags.HasFlag(MouseButtonFlags.LeftButtonDown))
            {
                _leftMouseDown = true;
            }
            else if (mouseInputEventArgs.ButtonFlags.HasFlag(MouseButtonFlags.LeftButtonUp))
            {
                _leftMouseDown = false;
            }

            if (mouseInputEventArgs.ButtonFlags.HasFlag(MouseButtonFlags.RightButtonDown))
            {
                _rightMouseDown = true;
            }
            else if (mouseInputEventArgs.ButtonFlags.HasFlag(MouseButtonFlags.RightButtonUp))
            {
                _rightMouseDown = false;
            }

            if (mouseInputEventArgs.X != 0 || mouseInputEventArgs.Y != 0)
            {
                if (_rightMouseDown)
                {
                    const double Sensitivity = 0.01f;

                    // mouse has moved
                    // rotate
                    var yawAmount = mouseInputEventArgs.X * Sensitivity;
                    var pitchAmount = mouseInputEventArgs.Y * Sensitivity;

                    _yaw += yawAmount;
                    _pitch += pitchAmount;
                    _pitch = Math.Max(-Math.PI / 2.0, Math.Min(0, _pitch));
                    _matricesNeedUpdating = true;
                }

                if (_leftMouseDown)
                {
                    const float Sensitivity = 0.01f;
                    var motionX = mouseInputEventArgs.X * Sensitivity * (1 / _scale);
                    var motionY = mouseInputEventArgs.Y * Sensitivity * (1 / _scale);

                    var motion = Vector3.Zero;

                    var upMotion = _up;
                    upMotion.Z = 0;
                    if (upMotion.LengthSquared() > MathUtil.ZeroTolerance)
                    {
                        upMotion = Vector3.Normalize(upMotion);
                        motion += upMotion * motionY;
                    }

                    var rightMotion = _right;
                    rightMotion.Z = 0;
                    if (rightMotion.LengthSquared() > MathUtil.ZeroTolerance)
                    {
                        rightMotion = Vector3.Normalize(rightMotion);
                        motion += rightMotion * motionX;
                    }

                    motion.Z = 0;
                    if (!Vector3.NearEqual(motion, Vector3.Zero, new Vector3(MathUtil.ZeroTolerance)))
                    {
                        Target += motion;
                        _matricesNeedUpdating = true;
                    }
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            if (!_matricesNeedUpdating) return;
            UpdateMatrices();
            _matricesNeedUpdating = false;
        }

        private void UpdateMatrices()
        {
            //Console.Out.WriteLine($"Pitch:{_pitch} Yaw:{_yaw}");
            var rotation = Matrix.RotationX((float)_pitch) * Matrix.RotationZ((float)_yaw);
            _forward = Vector3.TransformNormal(Vector3.UnitZ, rotation);
            _up = Vector3.TransformNormal(Vector3.UnitY, rotation);
            _right = Vector3.TransformNormal(Vector3.UnitX, rotation);

            var cameraPos = Target + (_forward * Distance);
            //Console.Out.WriteLine($"Camera:{cameraPos} Target:{Target} Up:{_up}");
            var view = Matrix.LookAtLH(cameraPos, Target, _up) * Matrix.Scaling(_scale);

            _cameraMatrices.View = view;
        }
        
    }
}