(() => {
    const search = document.getElementById('class-search');
    const rows = [...document.querySelectorAll('.class-row')];
    const groups = [...document.querySelectorAll('.namespace-group')];
    const showGenerated = document.getElementById('show-generated');
    const emptyResults = document.getElementById('empty-results');
    const themeToggle = document.getElementById('theme-toggle');

    const updateRows = () => {
        const query = search?.value.trim().toLocaleLowerCase() ?? '';
        let visibleCount = 0;
        groups.forEach(group => {
            const matchesNamespace = query.length > 0 && group.dataset.namespace.toLocaleLowerCase().includes(query);
            const isCollapsed = group.dataset.collapsed === 'true';
            let groupVisibleCount = 0;
            group.querySelectorAll('.class-row').forEach(row => {
                const matchesSearch = query.length === 0 || matchesNamespace || row.dataset.search.toLocaleLowerCase().includes(query);
                const isGenerated = row.dataset.generated === 'true';
                const visible = matchesSearch && (!isGenerated || showGenerated?.checked);
                row.hidden = !visible || isCollapsed;
                if (visible) {
                    visibleCount++;
                    groupVisibleCount++;
                }
            });
            group.hidden = groupVisibleCount === 0;
        });
        if (emptyResults) emptyResults.hidden = rows.length === 0 || visibleCount > 0;
    };

    groups.forEach(group => {
        const toggle = group.querySelector('.namespace-toggle');
        if (toggle) {
            toggle.addEventListener('click', () => {
                const isExpanded = toggle.getAttribute('aria-expanded') === 'true';
                toggle.setAttribute('aria-expanded', String(!isExpanded));
                group.dataset.collapsed = String(isExpanded);
                updateRows();
            });
        }
    });

    if (search) {
        search.addEventListener('input', updateRows);
    }

    if (showGenerated) {
        showGenerated.addEventListener('change', updateRows);
    }

    updateRows();

    if (themeToggle) {
        themeToggle.addEventListener('click', () => {
            const nextTheme = document.documentElement.dataset.theme === 'dark' ? 'light' : 'dark';
            document.documentElement.dataset.theme = nextTheme;
            const targetTheme = nextTheme === 'dark' ? 'light' : 'dark';
            themeToggle.textContent = `${targetTheme === 'light' ? 'Light' : 'Dark'} theme`;
            themeToggle.setAttribute('aria-label', `Switch to ${targetTheme} theme`);
        });
    }
})();