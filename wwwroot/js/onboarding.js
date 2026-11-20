(function () {
    var table = document.getElementById('lines-table');
    if (!table) return;

    var body = table.querySelector('tbody');
    var prefix = table.dataset.prefix;
    var max = parseInt(table.dataset.max || '0', 10);
    var template = document.getElementById('line-template');
    var addButton = document.getElementById('add-line');

    function rows() {
        return Array.prototype.slice.call(body.querySelectorAll('tr[data-row]'));
    }

    function reindex() {
        rows().forEach(function (row, index) {
            row.querySelectorAll('[name]').forEach(function (el) {
                el.name = el.name.replace(/\[\d+\]/, '[' + index + ']');
            });
        });
        updateTotals();
        if (addButton && max) addButton.disabled = rows().length >= max;
    }

    function updateTotals() {
        var grand = 0;
        rows().forEach(function (row) {
            var qty = parseFloat((row.querySelector('[data-qty]') || {}).value) || 0;
            var cost = parseFloat((row.querySelector('[data-cost]') || {}).value) || 0;
            var total = qty * cost;
            var cell = row.querySelector('[data-line-total]');
            if (cell) cell.textContent = total.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
            grand += total;
        });
        var grandCell = document.getElementById('grand-total');
        if (grandCell) grandCell.textContent = grand.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }

    if (addButton && template) {
        addButton.addEventListener('click', function () {
            if (max && rows().length >= max) return;
            var html = template.innerHTML.replace(/__INDEX__/g, rows().length);
            body.insertAdjacentHTML('beforeend', html);
            reindex();
        });
    }

    table.addEventListener('click', function (e) {
        var button = e.target.closest('[data-remove]');
        if (!button) return;
        if (rows().length <= 1) return;
        button.closest('tr').remove();
        reindex();
    });

    table.addEventListener('input', updateTotals);
    reindex();
})();
