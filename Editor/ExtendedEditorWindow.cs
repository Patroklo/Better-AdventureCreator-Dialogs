using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEditor;

namespace Dialogs
{

    public enum MouseButton
    {
        Left = 0,
        Right = 1,
        Middle = 2
    }

    public class Keyboard
    {

        public Keyboard()
        {
        }

        public Keyboard(Event evt)
        {
            this.Code = evt.keyCode;
            this.IsAlt = evt.alt;
            this.IsCapsLock = evt.capsLock;
            this.IsControl = evt.control;
            this.IsFunctionKey = evt.functionKey;
            this.IsNumeric = evt.numeric;
            this.IsShift = evt.shift;
            this.Modifiers = evt.modifiers;
        }

        public KeyCode Code { get; set; }

        public bool IsAlt { get; set; }

        public bool IsCapsLock { get; set; }

        public bool IsControl { get; set; }

        public bool IsFunctionKey { get; set; }

        public bool IsNumeric { get; set; }

        public bool IsShift { get; set; }

        public EventModifiers Modifiers { get; set; }
    }

    public abstract class ExtendedEditorWindow : EditorWindow
    {
        public Dictionary<EventType, Action> EventMap { get; set; }

        public ExtendedEditorWindow()
        {
            this.EventMap = new Dictionary<EventType, Action>
            {
                { EventType.ContextClick, this.OnContext },
                { EventType.Layout, this.OnLayout },
                { EventType.Repaint, this.OnRepaint },

                { EventType.KeyDown, () => {
                    this.OnKeyDown(new Keyboard(Event.current));
                }},

                { EventType.KeyUp, () => {
                    this.OnKeyUp(new Keyboard(Event.current));
                }},

                { EventType.MouseDown, () => {
                    this.OnMouseDown((MouseButton)Event.current.button, Event.current.mousePosition);
                }},

                    { EventType.MouseUp, () => {
                    this.OnMouseUp((MouseButton)Event.current.button, Event.current.mousePosition);
                }},
                { EventType.MouseDrag, () => {
                    this.OnMouseDrag((MouseButton)Event.current.button, Event.current.mousePosition,
                        Event.current.delta);
                }},

                { EventType.MouseMove, () => {
                    this.OnMouseMove(Event.current.mousePosition, Event.current.delta);
                }},

                { EventType.ScrollWheel, () => {
                    this.OnScrollWheel(Event.current.delta);
                }}
            };
        }

        protected virtual void OnGUI()
        {
            var controlId =
                GUIUtility.GetControlID(FocusType.Passive);

            var controlEvent =
                Event.current.GetTypeForControl(controlId);

            if (this.EventMap.ContainsKey(controlEvent))
            {
                this.EventMap[controlEvent].Invoke();
            }
        }

        protected void OnKeyDown(Keyboard keyboard)
        {
        }

        protected void OnKeyUp(Keyboard keyboard)
        {
        }

        protected virtual void OnMouseDown(MouseButton button, Vector2 position)
        {
        }

        protected virtual void OnMouseUp(MouseButton button, Vector2 position)
        {
        }

        protected virtual void OnMouseDrag(MouseButton button, Vector2 position, Vector2 delta)
        {
        }

        protected virtual void OnMouseMove(Vector2 position, Vector2 delta)
        {
        }

        protected virtual void OnContext()
        {
        }

        protected virtual void OnLayout()
        {
        }

        protected virtual void OnRepaint()
        {
        }

        protected virtual void OnScrollWheel(Vector2 delta)
        {
        }
    }

}

