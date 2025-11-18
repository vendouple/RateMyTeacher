document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('[data-rating-input]').forEach(container => {
        const hiddenInput = container.querySelector('input[type="hidden"][name$=".Stars"]');
        if (!hiddenInput) {
            return;
        }

        const liveRegion = container.querySelector('[data-rating-live]');
        const buttons = Array.from(container.querySelectorAll('.rating-star[data-value]'));

        const setValue = value => {
            if (!hiddenInput) {
                return;
            }

            hiddenInput.value = value;
            buttons.forEach(button => {
                const isActive = Number(button.dataset.value) <= value;
                button.classList.toggle('is-active', isActive);
                button.setAttribute('aria-pressed', isActive.toString());
            });

            if (liveRegion) {
                liveRegion.textContent = `Selected: ${value} star${value === 1 ? '' : 's'}`;
            }
        };

        buttons.forEach(button => {
            button.addEventListener('click', event => {
                event.preventDefault();
                const value = Number(button.dataset.value ?? hiddenInput.value);
                if (Number.isNaN(value)) {
                    return;
                }
                setValue(value);
            });
        });

        const initialValue = Number(hiddenInput.value || 5) || 5;
        setValue(initialValue);
    });
});
