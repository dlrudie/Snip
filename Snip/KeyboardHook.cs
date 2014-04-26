// http://www.liensberger.it/web/blog/?p=207

namespace Winter
{
    using System;
    using System.Windows.Forms;

    public sealed class KeyboardHook : IDisposable
    {
        /// <summary>
        /// Represents the window that is used internally to get the messages.
        /// </summary>
        private sealed class Window : NativeWindow, IDisposable
        {
            public Window()
            {
                // Create the handle for the window.
                this.CreateHandle(new CreateParams());
            }

            /// <summary>
            /// Overridden to get the notifications.
            /// </summary>
            /// <param name="m"></param>
            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);

                // Check if we got a hot key pressed.
                if (m.Msg == (uint)Globals.WindowMessage.Hotkey)
                {
                    // Get the keys.
                    Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);
                    ModifierHookKeys modifier = (ModifierHookKeys)((int)m.LParam & 0xFFFF);

                    // Invoke the event to notify the parent.
                    if (KeyPressed != null)
                    {
                        KeyPressed(this, new KeyPressedEventArgs(modifier, key));
                    }
                }
            }

            public event EventHandler<KeyPressedEventArgs> KeyPressed;

            #region IDisposable Members

            public void Dispose()
            {
                this.DestroyHandle();
            }

            #endregion
        }

        private Window window = new Window();
        private int currentId;

        public KeyboardHook()
        {
            // Register the event of the inner native window.
            this.window.KeyPressed += delegate(object sender, KeyPressedEventArgs args)
            {
                if (KeyPressed != null)
                {
                    KeyPressed(this, args);
                }
            };
        }

        /// <summary>
        /// Registers a hot key in the system.
        /// </summary>
        /// <param name="modifier">The modifiers that are associated with the hot key.</param>
        /// <param name="key">The key itself that is associated with the hot key.</param>
        public void RegisterHotkey(ModifierHookKeys modifier, Keys key)
        {
            // Increment the counter.
            this.currentId += 1;

            // Register the hot key.
            if (!UnsafeNativeMethods.RegisterHotKey(this.window.Handle, this.currentId, (uint)modifier, (uint)key))
            {
                throw new InvalidOperationException("Couldn't register the hotkey.");
            }
        }

        /// <summary>
        /// A hot key has been pressed.
        /// </summary>
        public event EventHandler<KeyPressedEventArgs> KeyPressed;

        #region IDisposable Members

        public void Dispose()
        {
            // Unregister all the registered hot keys.
            for (int i = this.currentId; i > 0; i--)
            {
                UnsafeNativeMethods.UnregisterHotKey(this.window.Handle, i);
            }

            // Dispose the inner native window.
            this.window.Dispose();
        }

        #endregion
    }

    /// <summary>
    /// Event Args for the event that is fired after the hot key has been pressed.
    /// </summary>
    public class KeyPressedEventArgs : EventArgs
    {
        private ModifierHookKeys modifier;
        private Keys key;

        internal KeyPressedEventArgs(ModifierHookKeys modifier, Keys key)
        {
            this.modifier = modifier;
            this.key = key;
        }

        public ModifierHookKeys Modifier
        {
            get
            {
                return this.modifier;
            }
        }

        public Keys Key
        {
            get
            {
                return this.key;
            }
        }
    }

    /// <summary>
    /// The enumeration of possible modifiers.
    /// </summary>
    [Flags]
    public enum ModifierHookKeys : int
    {
        Alt = 0x1,
        Control = 0x2,
        Shift = 0x4,
        Win = 0x8
    }
}
