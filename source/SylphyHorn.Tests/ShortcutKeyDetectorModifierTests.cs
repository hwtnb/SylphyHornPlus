using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using SylphyHorn.Services;
using Xunit;

namespace SylphyHorn.Tests
{
	public sealed class ShortcutKeyDetectorModifierTests
	{
		[Fact]
		public void ModifiersReleasedWhileTheSecureDesktopWasShownAreForgotten()
		{
			// Ctrl+Alt+Del: the key-up events of Ctrl and Alt never reach the hook.
			var pressed = new HashSet<Keys> { Keys.LControlKey, Keys.LMenu };

			ShortcutKeyDetector.RemoveReleasedModifiers(pressed, _ => false);

			Assert.Empty(pressed);
		}

		[Fact]
		public void ModifiersStillHeldDownAreKept()
		{
			var pressed = new HashSet<Keys> { Keys.LWin, Keys.LControlKey, Keys.LMenu };
			var heldDown = new HashSet<Keys> { Keys.LWin, Keys.LControlKey };

			ShortcutKeyDetector.RemoveReleasedModifiers(pressed, heldDown.Contains);

			Assert.Equal(new[] { Keys.LWin, Keys.LControlKey }, pressed.OrderBy(x => x));
		}

		[Fact]
		public void KeyStateIsNotQueriedWithoutPressedModifiers()
		{
			var queried = false;

			ShortcutKeyDetector.RemoveReleasedModifiers(new HashSet<Keys>(), _ => queried = true);

			Assert.False(queried);
		}

		[Fact]
		public void ShortcutMatchesAgainAfterStaleModifiersAreForgotten()
		{
			// Win+Ctrl+Right after Ctrl+Alt+Del: Alt was left in the set and the shortcut did not match.
			var pressed = new HashSet<Keys> { Keys.LControlKey, Keys.LMenu, Keys.LWin };
			var heldDown = new HashSet<Keys> { Keys.LControlKey, Keys.LWin };
			var expected = new ShortcutKeyPressedEventArgs(Keys.Right, new HashSet<Keys> { Keys.LControlKey, Keys.LWin }).ShortcutKey;

			Assert.NotEqual(expected, new ShortcutKeyPressedEventArgs(Keys.Right, pressed).ShortcutKey);

			ShortcutKeyDetector.RemoveReleasedModifiers(pressed, heldDown.Contains);

			Assert.Equal(expected, new ShortcutKeyPressedEventArgs(Keys.Right, pressed).ShortcutKey);
		}
	}
}
