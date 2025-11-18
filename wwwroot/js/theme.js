(function () {
	const storageKey = "rmt-theme";
	const root = document.documentElement;
	const prefersDarkMedia = window.matchMedia("(prefers-color-scheme: dark)");

	const readStoredTheme = () => {
		try {
			return window.localStorage.getItem(storageKey);
		} catch {
			return null;
		}
	};

	const persistTheme = (theme) => {
		try {
			window.localStorage.setItem(storageKey, theme);
		} catch {
			// Storage blocked; ignore persistence.
		}
	};

	const getPreferredTheme = () => readStoredTheme() || (prefersDarkMedia.matches ? "dark" : "light");

	const applyTheme = (theme) => {
		root.setAttribute("data-theme", theme);
		root.style.colorScheme = theme;
	};

	const updateToggle = (theme) => {
		const toggleButton = document.getElementById("themeToggle");
		if (!toggleButton) {
			return;
		}

		const icon = toggleButton.querySelector("[data-theme-icon]");
		const nextTheme = theme === "dark" ? "light" : "dark";
		toggleButton.setAttribute("aria-label", `Switch to ${nextTheme} theme`);
		toggleButton.setAttribute("data-next-theme", nextTheme);
		if (icon) {
			icon.textContent = theme === "dark" ? "☀️" : "🌙";
		}
	};

	const setTheme = (theme) => {
		applyTheme(theme);
		persistTheme(theme);
		updateToggle(theme);
	};

	document.addEventListener("DOMContentLoaded", () => {
		const toggleButton = document.getElementById("themeToggle");
		if (!toggleButton) {
			return;
		}

		toggleButton.hidden = false;
		updateToggle(getPreferredTheme());

		toggleButton.addEventListener("click", () => {
			const currentTheme = root.getAttribute("data-theme") || getPreferredTheme();
			const nextTheme = currentTheme === "dark" ? "light" : "dark";
			setTheme(nextTheme);
		});
	});

	prefersDarkMedia.addEventListener("change", (event) => {
		const stored = readStoredTheme();
		if (stored) {
			return; // Respect explicit user preference.
		}
		setTheme(event.matches ? "dark" : "light");
	});

	// Apply preferred theme immediately on load.
	applyTheme(getPreferredTheme());
})();
