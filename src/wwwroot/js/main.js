document.addEventListener('DOMContentLoaded', () => {
    const themeButton = document.getElementById('ThemeButton');

    if (themeButton) {
        themeButton.addEventListener('click', () => {
            const theme = document.querySelector('html').getAttribute('data-theme');
            document.querySelector('html').setAttribute('data-theme', theme === 'dark' ? 'light' : 'dark');
        });
    }
});
