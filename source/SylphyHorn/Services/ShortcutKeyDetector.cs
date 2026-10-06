using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Windows.Forms;
using SylphyHorn.Services.Mouse;

namespace SylphyHorn.Services
{
	/// <summary>
	/// Provides the function to detect a shortcut key ([modifier key(s)] + [key] style) by use of global key hook.
	/// </summary>
	public class ShortcutKeyDetector : IDisposable
	{
		private readonly HashSet<Keys> _pressedModifiers = new HashSet<Keys>();
		private readonly MouseShortcutState _mouseState = new MouseShortcutState();
		private readonly Func<Keys, ICollection<Keys>, bool> _publishButtonPressed;
		private readonly Func<Keys, ICollection<Keys>, bool> _publishButtonUp;
		private readonly KeyboardInterceptor _keyInterceptor = new KeyboardInterceptor();
		private readonly MouseInterceptor _mouseInterceptor = new MouseInterceptor();

		private bool _started;
		private bool _suspended;
		private bool _disposed;

		/// <summary>
		/// Occurs when detects a shortcut key.
		/// </summary>
		public event EventHandler<ShortcutKeyPressedEventArgs> KeyPressed;
		public event EventHandler<ShortcutKeyPressedEventArgs> KeyUp;
		public event EventHandler<ShortcutKeyPressedEventArgs> ButtonPressed;
		public event EventHandler<ShortcutKeyPressedEventArgs> ButtonUp;

		public ShortcutKeyDetector()
		{
			this._publishButtonPressed = this.PublishButtonPressed;
			this._publishButtonUp = this.PublishButtonUp;
			this._keyInterceptor.KeyDown += this.InterceptorOnKeyDown;
			this._keyInterceptor.KeyUp += this.InterceptorOnKeyUp;
			this._mouseInterceptor.MouseDown += this.InterceptorOnMouseDown;
			this._mouseInterceptor.MouseUp += this.InterceptorOnMouseUp;
			this._mouseInterceptor.WheelDown += this.InterceptorOnMouseWheel;
			this._mouseInterceptor.WheelUp += this.InterceptorOnMouseWheel;
		}

		public void Start()
		{
			if (this._disposed)
			{
				throw new ObjectDisposedException(nameof(ShortcutKeyDetector));
			}

			if (!this._started)
			{
				this._keyInterceptor.StartCapturing();
				try
				{
					this._mouseInterceptor.StartCapturing();
				}
				catch (Exception startException)
				{
					try
					{
						this._keyInterceptor.StopCapturing();
					}
					catch (Exception rollbackException)
					{
						try
						{
							startException.Data["KeyboardHookRollbackException"] = rollbackException;
						}
						catch
						{
							// Never replace the original mouse hook failure.
						}
					}

					throw;
				}

				this._started = true;
			}

			this._suspended = false;
		}

		public void Stop()
		{
			this._suspended = true;
			this._pressedModifiers.Clear();
			this._mouseState.Clear();
		}

		private void InterceptorOnKeyDown(object sender, KeyEventArgs args)
		{
			if (this._suspended) return;

			RemoveReleasedModifiers(this._pressedModifiers, IsKeyDown);

			if (args.KeyCode.IsModifyKey())
			{
				this._pressedModifiers.Add(args.KeyCode);
			}
			else
			{
				var pressedEventArgs = new ShortcutKeyPressedEventArgs(args.KeyCode, this._pressedModifiers);
				this.KeyPressed?.Invoke(this, pressedEventArgs);
				if (pressedEventArgs.Handled) args.SuppressKeyPress = true;
			}
		}

		private void InterceptorOnKeyUp(object sender, KeyEventArgs args)
		{
			if (this._suspended) return;

			//if (this._pressedModifiers.Count == 0) return;

			if (args.KeyCode.IsModifyKey())
			{
				this._pressedModifiers.Remove(args.KeyCode);
			}
			else
			{
				RemoveReleasedModifiers(this._pressedModifiers, IsKeyDown);

				var pressedEventArgs = new ShortcutKeyPressedEventArgs(args.KeyCode, this._pressedModifiers);
				this.KeyUp?.Invoke(this, pressedEventArgs);
				if (pressedEventArgs.Handled) args.SuppressKeyPress = true;
			}
		}

		/// <summary>
		/// Forgets modifier keys that are no longer held down.
		/// </summary>
		/// <remarks>
		/// While the secure desktop is shown (Ctrl+Alt+Del, UAC prompts, the lock screen), key-up events are not delivered
		/// to the global hook. Without this, the modifiers pressed to open it (e.g. Ctrl and Alt) would stay registered
		/// and no shortcut key would match afterward.
		/// The detector never suppresses modifier key-down events, so the asynchronous key state reflects them.
		/// </remarks>
		internal static void RemoveReleasedModifiers(ISet<Keys> pressedModifiers, Func<Keys, bool> isKeyDown)
		{
			if (pressedModifiers.Count == 0) return;

			foreach (var key in pressedModifiers.Where(x => !isKeyDown(x)).ToArray())
			{
				pressedModifiers.Remove(key);
			}
		}

		private static bool IsKeyDown(Keys key)
		{
			return (Interop.NativeMethods.GlobalHook.GetAsyncKeyState((int)key) & 0x8000) != 0;
		}

		private void InterceptorOnMouseDown(ref MouseState state)
		{
			if (this._suspended) return;

			this._mouseState.TryProcessButtonDown(
				state.KeyCode,
				this._publishButtonPressed,
				ref state.Handled);
		}

		private void InterceptorOnMouseUp(ref MouseState state)
		{
			if (this._suspended) return;

			this._mouseState.TryProcessButtonUp(
				state.KeyCode,
				this._publishButtonUp,
				ref state.Handled);
		}

		private void InterceptorOnMouseWheel(ref MouseState state)
		{
			if (this._suspended) return;

			this._mouseState.TryProcessWheel(
				state.Stroke,
				state.KeyCode,
				this._publishButtonPressed,
				ref state.Handled);
		}

		private bool PublishButtonPressed(Keys keyCode, ICollection<Keys> modifiers)
		{
			var pressedEventArgs = new ShortcutKeyPressedEventArgs(keyCode, modifiers);
			this.ButtonPressed?.Invoke(this, pressedEventArgs);
			return pressedEventArgs.Handled;
		}

		private bool PublishButtonUp(Keys keyCode, ICollection<Keys> modifiers)
		{
			var pressedEventArgs = new ShortcutKeyPressedEventArgs(keyCode, modifiers);
			this.ButtonUp?.Invoke(this, pressedEventArgs);
			return pressedEventArgs.Handled;
		}

		public void Dispose()
		{
			if (this._disposed)
			{
				return;
			}

			Exception exception = null;
			try
			{
				this._keyInterceptor.Dispose();
			}
			catch (Exception ex)
			{
				exception = ex;
			}

			try
			{
				this._mouseInterceptor.Dispose();
			}
			catch (Exception ex)
			{
				if (exception == null)
				{
					exception = ex;
				}
			}

			if (exception != null)
			{
				ExceptionDispatchInfo.Capture(exception).Throw();
			}

			this._started = false;
			this._suspended = true;
			this._pressedModifiers.Clear();
			this._mouseState.Clear();
			this._disposed = true;
		}
	}
}
