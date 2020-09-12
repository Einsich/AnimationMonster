using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows.Input;
using ECS;
namespace Engine
{

    static class Input
    {
        struct KeyState
        {
            public bool pressed;
            public float time;
        }
        const int KeyCount = 200;
        static KeyState[] keys = new KeyState[KeyCount];
        public static Vector2 mousePosition { get; private set; }
        static Vector2 delta;
        public static Vector2 mouseDelta => (Time.time - lastMouseMoveTime) <= Time.deltaTime? delta : Vector2.Zero;
        static float lastMouseMoveTime = -1;

        const float wheelPeriod = 0.15f, wheelDecay = 0.2f;
        static float mouseWheelTime = -1, wheelSign = 0;
        public static float wheelDelta => (Time.time - mouseWheelTime < wheelPeriod ? 1 : Mathf.Clamp0((wheelPeriod - (Time.time - mouseWheelTime)) / wheelDecay))* wheelSign;

        static MouseButtonState[] mouseState = new MouseButtonState[3];
        public static KeyActions KeyAction { get; } = new KeyActions();
        static Input()
        {
            for (int i = 0; i < keys.Length; i++)
                keys[i].time = -10000;
        }

        public static float GetKey(Key key)
        {
            int i = (int)key;
            if (keys.Length <= i)
                throw new Exception("Key array is small for key " + key.ToString());
            KeyState state = keys[i];
            float dt = (Time.time - state.time) * 2.0f;
            return state.pressed ? Mathf.Clamp1(dt) : Mathf.Clamp0(1 - dt);

        }

        
        public static bool MouseDown(MouseButton button) => mouseState[(int)button] == MouseButtonState.Pressed;
        public static bool MouseUp(MouseButton button) => mouseState[(int)button] == MouseButtonState.Released;
        public static void KeyDownEventListener(object sender, KeyEventArgs args)
        {
            int i = (int)args.Key;
            if (keys.Length <= i)
                throw new Exception("Key array is small for key " + args.Key.ToString());
            if (!keys[i].pressed)
            {
                keys[i].pressed = true;
                keys[i].time = Time.time;
                KeyAction[i]?.Invoke(true);
            }
        }
        public static void KeyUpEventListener(object sender, KeyEventArgs args)
        {
            int i = (int)args.Key;
            if (keys.Length <= i)
                throw new Exception("Key array is small for key " + args.Key.ToString());
            if (keys[i].pressed)
            {
                keys[i].pressed = false;
                keys[i].time = Time.time;
                KeyAction[i]?.Invoke(false);
            }
        }
        public static void MouseDownEventListener(object sender, MouseButtonEventArgs args)
        {
            mouseState[0] = args.LeftButton;
            mouseState[1] = args.RightButton;
            mouseState[2] = args.MiddleButton;
        }
        public static void MouseUpEventListener(object sender, MouseButtonEventArgs args)
        {
            mouseState[0] = args.LeftButton;
            mouseState[1] = args.RightButton;
            mouseState[2] = args.MiddleButton;
        }
        public static void MouseMoveEventListener(object sender, MouseEventArgs args)
        {
            var p = args.GetPosition(sender as System.Windows.IInputElement);
            Vector2 m = new Vector2((float)p.X, (float)p.Y);
            delta = mousePosition - m;
            mousePosition = m;
            lastMouseMoveTime = Time.time;
        }
        public static void MouseEnterEventListener(object sender, MouseEventArgs args)
        {
            var p = args.GetPosition(sender as System.Windows.IInputElement);
            mousePosition = new Vector2((float)p.X, (float)p.Y);
        }
        public static void MouseLeaveEventListener(object sender, MouseEventArgs args)
        {
            var p = args.GetPosition(sender as System.Windows.IInputElement);
            mousePosition = new Vector2((float)p.X, (float)p.Y);
        }
        public static void MouseWheelEventListener(object sender, MouseWheelEventArgs args)
        {
            mouseWheelTime = Time.time;
            wheelSign = args.Delta < 0 ? -1 : 1;
        }
        public class KeyActions
        {
            public Action<bool>[] KeyAction = new Action<bool>[KeyCount];
            public Action<bool> this[Key key]
            {
                get => KeyAction[(int)key];
                set => KeyAction[(int)key] = value;
            }
            public Action<bool> this[int key]
            {
                get => KeyAction[(int)key];
                set => KeyAction[(int)key] = value;
            }
        }
    }
    public enum MouseButton
    {
        Left,
        Right,
        Middle
    }
}
